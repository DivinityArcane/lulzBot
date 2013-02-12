/*  lulzBot - a C# bot for dAmn.
 * 
 * Project creation date: Dec. 13th, 2012.
 * 
 * Authors: DivinityArcane, OrrinFox.
 * 
 * Desc.: The main purpose of this bot is to teach the basics of C#.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace lulzbot
{
    class Program
    {
        // This boolean controls whether or not the bot is allowed to run.
        public static bool Running = true;

        // Are we in debug mode?
        public static bool Debug = false;

        // This is our configuration.
        private static Config Config = new Config();

        // Our OS string
        public static String OS = Environment.OSVersion.ToString();

        // This is our bot object.
        public static Bot Bot = null;

        // Force the bot to reconnect?
        public static bool ForceReconnect = false;

        // Our bot thread!
        private static Thread _thread;

        // Wait event. Helps keep certain events in order.
        public static ManualResetEvent wait_event;

        // Bot related globals.
        public static DateTime StartTime = DateTime.UtcNow;
        public static int Disconnects = 0;
        public static ulong bytes_sent = 0, bytes_received = 0;
        public static List<String> OfficialChannels = new List<String>() { "#devart", "#help", "#mnadmin", "#seniors", "#communityrelations" };
        public const String BotName = "lulzBot";
        public const String Version = "0.5a";

        static void Main (string[] args)
        {
             /* Well, first off, the bot is _not_ going to be in the main file.
             * Why? That's silly. I don't like doing that. OOP, man. OOP.
             * Anyway, the Bot will be a separate class, and hence, object.
             * 
             * Of course, it will be started in a separate thread, but this main
             *  class will be in control of when the program ultimately ends.
             * 
             * For example: If you were to set the variable "Running" to false, 
             *  the bot instance would be killed off and the application will exit.
             *  
             * This ensures that there is always a reasonable way to kill of the bot.
             * 
             * Maybe that's just me though. -DivinityArcane */

            ///TODO: Fix the damn botinfo, make it request on old info.
            // Just a bit of a simple title.
            ConIO.Write (String.Format ("{0}, version {1}", BotName, Version));
            ConIO.Write ("Authors: DivinityArcane, OrrinFox.");
            
            Type _monotype = Type.GetType ("Mono.Runtime");
            if (null != _monotype)
            {
                System.Reflection.MethodInfo _mono_dsp_name = _monotype.GetMethod("GetDisplayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (null != _mono_dsp_name)
                    OS += String.Format (" (Running in Mono {0})", _mono_dsp_name.Invoke(null, null));
                else
                    OS += " (Running in Mono)";
            }

            // Check for debug mode!
            if (args.Length >= 1)
            {
                foreach (String arg in args)
                {
                    if (arg == "--debug")
                    {
                        ConIO.Write("Debug mode is enabled!");
                        Debug = true;
                    }
                    else if (arg == "--help")
                    {
                        ConIO.Write("To enable debug mode use the command line switch --debug");
                        Environment.Exit(0);
                    }
                }
            }

            // If in debug mode, output CWD
            if (Debug)
                ConIO.Write("Running in directory: " + Environment.CurrentDirectory, "Debug");

            // First things first: We need a config file! If we don't have one, make one.
            if (!File.Exists("./Config.dat"))
            {
                ConIO.Write("Looks like you don't have a config file. Let's make one!");
                ConIO.Write("I'm going to need some basic details about you and the bot.");
                Config.Username = ConIO.Read("What is the bot's dA username?");
                Config.Password = ConIO.Read("What is the bot's dA password?");
                Config.Owner = ConIO.Read("What is your dA username?");
                Config.Trigger = ConIO.Read("What is the bot's trigger?");

                // Channels need to be split, so let's just get them one by one.
                String channel = "none";

                ConIO.Write("OK. What channels will the bot join? One at a time, please.");
                ConIO.Write("When you're finished, just hit enter on an empty line.");

                while (!String.IsNullOrWhiteSpace(channel))
                {
                    if (!channel.StartsWith("#"))
                    {
                        ConIO.Write("Valid channel names start with #");
                    }
                    else if (!Config.Channels.Contains(channel.ToLower()))
                    {
                        Config.Channels.Add(channel.ToLower());
                    }
                    else
                    {
                        ConIO.Write("You already added that channel!");
                    }
                    channel = ConIO.Read("Add a channel?");
                }

                if (Config.Channels.Count <= 0)
                {
                    ConIO.Write("No channels added. Defaulting to #Botdom");
                    Config.Channels.Add("#Botdom");
                }

                ConIO.Write("That'll do it! Saving the config and continuing.");
                Config.Save("./Config.dat");
            }
            else
            {
                ConIO.Write("Configuration exists, loading it...");
                if (Config.Load("./Config.dat"))
                {
                    if (String.IsNullOrWhiteSpace(Config.Username) || String.IsNullOrWhiteSpace(Config.Password) || String.IsNullOrWhiteSpace(Config.Owner) || String.IsNullOrWhiteSpace(Config.Trigger))
                    {
                        ConIO.Write("Config data was null. Clearing the config file. Please restart the bot and reconfigure it.");
                        File.Delete("./Config.dat");

                        // Exit the app.
                        ConIO.Read("Press return/enter to close this window...");
                        Environment.Exit(-1);
                    }

                    ConIO.Write("Config loaded for: " + Config.Username);
                }
                else
                {
                    ConIO.Write("Something went wrong with the config!");
                    ConIO.Write("Please delete Config.dat and restart the bot.");

                    // Exit the app.
                    ConIO.Read("Press return/enter to close this window...");
                    Environment.Exit(-1);
                }
            }

            // Initialize events system
            Events.InitEvents();

            // Initialize the tablump parser
            Tools.InitLumps();

            // Initialize the wait event
            wait_event = new ManualResetEvent(true);

            // Ok, let's fire up the bot!
            // I considered passing the config as a reference, but there's no point.
            // Instead, let's just copy the config to the bot.
            _thread = new Thread(new ThreadStart(Start));
            _thread.IsBackground = false;
            _thread.Start();

            // We could call Bot.Connect() or whatever from here, but, eh. Let's do it
            //  from within the constructor of Bot() instead.

            while (Running)
            {
                // Wait for a signal
                wait_event.WaitOne();

                // Check if we need to reconnect
                if (ForceReconnect)
                {
                    ForceReconnect = false;
                    _thread.Abort();
                    if (!Running)
                        break;
                    _thread = new Thread(new ThreadStart(Start));
                    _thread.IsBackground = false;
                    _thread.Start();
                }

                // Wait for another signal
                wait_event.Reset();
            }

            // Make sure they see whatever happened first.
            ConIO.Read("Press return/enter to close this window...");

            // Make sure all threads are killed off, and exit. Exit code 0 = OK
            Environment.Exit(0);
        }

        /// <summary>
        /// Initializes a new bot instance
        /// </summary>
        public static void Start()
        {
            Program.Bot = null;
            Program.Bot = new Bot(Config);
        }
    }
}
