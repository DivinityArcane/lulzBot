using lulzbot.Extensions;
using lulzbot.Networking;
using System;
using System.Threading;

namespace lulzbot
{
    public class Bot
    {
        // These are our dAmn server variables.
        //private const String _hostname = "chat.deviantart.com";
        //private const int _port = 3900;

        // Our socket wrapper/object.
        private SocketWrapper Socket = null;

        public int QueuedIn
        {
            get
            {
                return Socket.QueuedIn;
            }
        }

        public int QueuedOut
        {
            get
            {
                return Socket.QueuedOut;
            }
        }

        // This is our config.
        public Config Config;

        // Basic vars that will be saved later
        public bool AutoReJoin = true;

        // Are we shutting down?
        public bool Quitting = false;

        // Core extensions
        public static Core Core;
        public static BDS BDS;
        public static Logger Logger;
        public static ExtensionContainer Extensions;
        public static Users Users;
        public static Colors Colors;
        public static AI AI;

        // Whether or not we can loop
        private bool can_loop = false;
        private Thread _loop_thread;

        // Wait event for the thread
        //public static ManualResetEvent wait_event;

        // Bot vars!
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Bot uptime in seconds.
        /// </summary>
        public int uptime
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - Program.StartTime).TotalSeconds);
            }

            set { }
        }

        /// <summary>
        /// Seconds since the unix epoch
        /// </summary>
        public static int EpochTimestamp
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - _epoch).TotalSeconds);
            }

            set { }
        }

        /// <summary>
        /// Milliseconds since the unix epoch
        /// </summary>
        public static int EpochTimestampMS
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - _epoch).TotalMilliseconds);
            }

            set { }
        }

        /// <summary>
        /// Ticks when we were pinged.
        /// </summary>
        public int _pinged = 0;


        /// <summary>
        /// Constructor. Spawn a new bot instance
        /// </summary>
        /// <param name="config">Configuration object</param>
        public Bot (Config config, string host, int port)
        {
            // Initialize the wait handler
            //wait_event = new ManualResetEvent(false);

            // Assign the config to our class variable
            this.Config = config;

            // Check if the authtoken stored is empty
            if (String.IsNullOrWhiteSpace(Config.Authtoken))
            {
                ConIO.Write("We don't have an authtoken. Grabbing one...");
                Config.Authtoken = AuthToken.Grab(Config.Username, Config.Password);

                if (String.IsNullOrWhiteSpace(Config.Authtoken))
                {
                    ConIO.Write("Invalid username or password! Deleting config...");
                    System.IO.File.Delete(@"./Config.dat");
                    Program.Running = false;
                    Program.wait_event.Set();
                    return;
                }
                else
                {
                    ConIO.Write("Got an authtoken!");
                    Config.Save("Config.dat");
                }
            }

            // Make sure events are clear.
            Events.ClearEvents();

            // Initialize the Core extensions
            Core = new Core();
            BDS = new BDS();
            Logger = new Logger();
            Extensions = new ExtensionContainer();
            Users = new Users(this.Config.Owner);
            Colors = new Colors();
            AI = new AI();

            // Now, let's initialize the socket.
            Socket = new SocketWrapper();
            Socket.Connect(host, port);

            can_loop = true;

            // Start a new thread for our MainLoop method
            _loop_thread = new Thread(new ThreadStart(MainLoop));
            _loop_thread.IsBackground = false;
            _loop_thread.Start();
        }

        /// <summary>
        /// Main bot loop
        /// </summary>
        private void MainLoop ()
        {
            // Woo! Loop!
            while (can_loop)
            {
                // Wait for a signal
                //wait_event.WaitOne();

                if (Socket.QueuedOut > 0)
                {
                    Socket.PopPacket();
                }

                dAmnPacket packet = null;

                if ((packet = Socket.Dequeue()) != null)
                {
                    if (packet != null)
                    {
                        Program.packets_in++;
                        // Process the packet
                        if (packet.Command == "recv")
                        {
                            new Thread (() => Events.CallEvent("recv_" + packet.SubCommand, packet)).Start ();
                        }
                        else
                        {
                            new Thread (() => Events.CallEvent(packet.Command, packet)).Start ();
                        }
                    }
                }

                // Go back to waiting.
                //wait_event.Reset();

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Reconnect the bot
        /// </summary>
        public void Reconnect ()
        {
            if (Quitting)
                return;

            // No reason to call this now. It might cause issues.
            //Events.ClearEvents();

            ConIO.Write("Reconnecting in 5 seconds!");

            Thread.Sleep(5000);

            ConIO.Write("Reconnecting...");

            Program.ForceReconnect = true;
            Socket.Close();
            can_loop = false;
            Program.wait_event.Set();
        }

        /// <summary>
        /// Joins a dAmn channel
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void Join (String channel)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Join(channel));
        }

        /// <summary>
        /// Parts a dAmn channel
        /// </summary>
        /// <param name="channel">Channel to part</param>
        public void Part (String channel)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            if (channel.ToLower() != "chat:datashare")
                Send(dAmnPackets.Part(channel));
        }

        /// <summary>
        /// Sends a message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void Say (String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            if (Colors.Config.Enabled)
                message += Colors.ColorTag;
            Send(dAmnPackets.Message(channel, message));
        }

        /// <summary>
        /// Sends a non-parsed message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void NPSay (String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.NPMessage(channel, message));
        }

        /// <summary>
        /// Sends an action to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void Act (String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Action(channel, message));
        }

        public void Kick (String channel, String who, String reason)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Kick(channel, who, reason));
        }

        public void Ban (String channel, String who)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Ban(channel, who));
        }

        public void UnBan (String channel, String who)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.UnBan(channel, who));
        }

        public void Admin (String channel, String command)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Admin(channel, command));
        }

        public void Kill (String who, String reason)
        {
            Send(dAmnPackets.Kill(who, reason));
        }

        public void Promote (String channel, String who, String privclass)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Promote(channel, who, privclass));
        }

        public void Demote (String channel, String who, String privclass)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Demote(channel, who, privclass));
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in byte array form</param>
        public void Send (byte[] packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in string form</param>
        public void Send (String packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends the disconnect packet.
        /// </summary>
        public void Disconnect ()
        {
            Send("disconnect\n\0");
        }

        /// <summary>
        /// Closes down the bot.
        /// </summary>
        public void Close ()
        {
            Socket.Close();
            can_loop = false;
            Program.Running = false;
            Program.wait_event.Set();
        }

        /// <summary>
        /// Return the server endpoint in IP:PORT format
        /// </summary>
        /// <returns>Server endpoint</returns>
        public String Endpoint ()
        {
            return Socket.Endpoint();
        }
    }
}
