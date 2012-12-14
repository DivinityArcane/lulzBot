/*  lulzBot - a C# bot for dAmn.
 * 
 * Project creation date: Dec. 13th, 2012.
 * 
 * Authors: DivinityArcane, OrrinFox.
 * 
 * Desc.: The main purpose of this bot is to teach the basics of C#.
 */

using System;
using System.IO;

namespace lulzbot
{
    class Program
    {
        // This boolean controls whether or not the bot is allowed to run.
        public static bool Running = true;

        // This is our configuration.
        public static Config Config = new Config();

        // Bot related globals.
        public const String BotName = "lulzBot";
        public const String Version = "0.1a";

        static void Main(string[] args)
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

            // Just a bit of a simple title.
            ConIO.Write(String.Format("{0}, version {1}", BotName, Version));
            ConIO.Write("Authors: DivinityArcane, OrrinFox.");

            // First things first: We need a config file! If we don't have one, make one.
            if (!File.Exists("Config.dat"))
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

                if (Config.Channels.Count <= 1)
                {
                    ConIO.Write("No channels added. Defaulting to #Botdom");
                    Config.Channels.Add("#Botdom");
                }

                ConIO.Write("That'll do it! Saving the config and continuing.");
                Config.Save("Config.dat");
            }
            else
            {
                ConIO.Write("Configuration exists, loading it...");
                if (Config.Load("Config.dat"))
                {
                    ConIO.Write("Bot's username: " + Config.Username);
                    ConIO.Write("Bot's trigger : " + Config.Trigger);
                    ConIO.Write("Bot's owner   : " + Config.Owner);
                    ConIO.Write("Bot's autojoin: " + String.Join(", ", Config.Channels));
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

            while (Running)
            {
                // Do non bot related stuff? Maybe keep track of memory usage with
                //  Marshall.SizeOf? Though, that's "unsafe" code and requires that
                //  we enable that in settings. Who knows.

                // Make the main thread sleep for 100ms. So max of 10 iterations/second.
                System.Threading.Thread.Sleep(100);
            }

            // Make sure they see whatever happened first.
            ConIO.Read("Press return/enter to close this window...");

            // Make sure all threads are killed off, and exit. Exit code 0 = OK
            Environment.Exit(0);
        }
    }
}
