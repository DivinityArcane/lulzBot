using lulzbot.Extensions;
using lulzbot.Networking;
using System;
using System.Threading;

namespace lulzbot
{
    public class Bot
    {
        // These are our dAmn server variables.
        private const String _hostname = "chat.deviantart.com";
        private const int _port = 3900;

        // Our socket wrapper/object.
        private SocketWrapper Socket = null;

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

        // Whether or not we can loop
        private bool can_loop = false;
        private Thread _loop_thread;

        // Wait event for the thread
        public static ManualResetEvent wait_event;

        // Bot vars!
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Timestamp of the time the bot started.
        /// </summary>
        private DateTime _started = DateTime.UtcNow;

        /// <summary>
        /// Bot uptime in seconds.
        /// </summary>
        public int uptime
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - _started).TotalSeconds);
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
        /// Ticks when we were pinged.
        /// </summary>
        public int _pinged = 0;


        /// <summary>
        /// Constructor. Spawn a new bot instance
        /// </summary>
        /// <param name="config">Configuration object</param>
        public Bot(Config config)
        {
            // Initialize the wait handler
            wait_event = new ManualResetEvent(false);

            // Assign the config to our class variable
            this.Config = config;

            // Check if the authtoken stored is empty
            if (String.IsNullOrWhiteSpace(Config.Authtoken))
            {
                ConIO.Write("We don't have an authtoken. Grabbing one...");
                Config.Authtoken = AuthToken.Grab(Config.Username, Config.Password);

                if (String.IsNullOrWhiteSpace(Config.Authtoken))
                {
                    ConIO.Write("Invalid username or password!");
                    Program.Running = false;
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
            Core    = new Core();
            BDS     = new BDS();
            Logger  = new Logger();

            // Now, let's initialize the socket.
            Socket = new SocketWrapper();
            Socket.Connect(_hostname, _port);

            can_loop = true;

            // Start a new thread for our MainLoop method
            _loop_thread = new Thread(new ThreadStart(MainLoop));
            _loop_thread.IsBackground = false;
            _loop_thread.Start();
        }

        /// <summary>
        /// Main bot loop
        /// </summary>
        private void MainLoop()
        {
            // Woo! Loop!
            while (can_loop)
            {
                // Wait for a signal
                wait_event.WaitOne();

                dAmnPacket packet = null;

                while ((packet = Socket.Dequeue()) != null)
                {
                    if (packet != null)
                    {
                        // Process the packet
                        if (packet.Command == "recv")
                        {
                            Events.CallEvent("recv_" + packet.SubCommand, packet);
                        }
                        else
                        {
                            Events.CallEvent(packet.Command, packet);
                        }
                    }
                }

                // Go back to waiting.
                wait_event.Reset();
            }
        }

        /// <summary>
        /// Reconnect the bot
        /// </summary>
        public void Reconnect()
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
            Socket = null;
            can_loop = false;
            Program.wait_event.Set();
            _loop_thread.Join();
        }

        /// <summary>
        /// Joins a dAmn channel
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void Join(String channel)
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
        public void Part(String channel)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Part(channel));
        }

        /// <summary>
        /// Sends a message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void Say(String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Message(channel, message));
        }

        /// <summary>
        /// Sends a non-parsed message to the specified channel
        /// </summary>
        /// <param name="channel">Channel to send to</param>
        /// <param name="message">Message to say</param>
        public void NPSay(String channel, String message)
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
        public void Act(String channel, String message)
        {
            if (channel.StartsWith("#"))
            {
                channel = String.Format("chat:{0}", channel.Substring(1));
            }
            Send(dAmnPackets.Action(channel, message));
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in byte array form</param>
        public void Send(byte[] packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends a packet to the server
        /// </summary>
        /// <param name="packet">dAmnPacket in string form</param>
        public void Send(String packet)
        {
            Socket.Send(packet);
        }

        /// <summary>
        /// Sends the disconnect packet.
        /// </summary>
        public void Disconnect()
        {
            Send("disconnect\n\0");
        }

        /// <summary>
        /// Closes down the bot.
        /// </summary>
        public void Close()
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
        public String Endpoint()
        {
            return Socket.Endpoint();
        }
    }
}
