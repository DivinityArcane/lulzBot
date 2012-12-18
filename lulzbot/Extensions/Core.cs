using lulzbot.Networking;
using System;

namespace lulzbot.Extensions
{
    public class Core
    {
        /// <summary>
        /// Constructor. Add basic events.
        /// </summary>
        public Core()
        {
            // Format is simple:
            // Events.AddEvent("event_name", event_obj);
            // Where event_obj is:
            //  new Event(this, "function_name")
            // To keep things clean, start function names for events with evt_
            Events.AddEvent("on_connect",   new Event(this, "evt_connect"));
            Events.AddEvent("dAmnServer",   new Event(this, "evt_preauth"));
            Events.AddEvent("login",        new Event(this, "evt_login"));
            Events.AddEvent("join",         new Event(this, "evt_join"));
            Events.AddEvent("part",         new Event(this, "evt_part"));
            Events.AddEvent("property",     new Event(this, "evt_property"));
            Events.AddEvent("recv_msg",     new Event(this, "evt_recv_msg"));
            Events.AddEvent("recv_action",  new Event(this, "evt_recv_action"));
            Events.AddEvent("ping",         new Event(this, "evt_ping"));

            // Again, simple format:
            // Events.AddCommand("command_name", cmd_obj);
            // Where cmd_obj is:
            //  new Command(this, "function_name", "Your_dA_username", "A simple help msg.", minimum_privs, "Description")
            // minimum_privs = minimum privilege level. 25 = guests, 50 = members, 75 = opers, 99 = admins, 100 = owner.
            Events.AddCommand("about",      new Command(this, "cmd_about", "DivinityArcane", "No help.", 25, "Displays information about the bot."));
            Events.AddCommand("ping",       new Command(this, "cmd_ping", "DivinityArcane", "No help.", 25, "Tests the latency between the bot and the server."));
        }

        #region Commands
        /// <summary>
        /// About command!
        /// </summary>
        public void cmd_about(Bot bot, String ns, String msg, dAmnPacket packet)
        {
            bot.Say(ns, String.Format("<b>&raquo; {0} v{1}</b> - <i>\"Embrace the lulz. &trade;\"</i><br/><b>&raquo; Written by:</b> :devDivinityArcane:, :devOrrinFox:<br/><b>&raquo; Owned by:</b> :dev{2}:", Program.BotName, Program.Version, bot.Config.Owner));
        }

        /// <summary>
        /// Ping command!
        /// </summary>
        public void cmd_ping(Bot bot, String ns, String msg, dAmnPacket packet)
        {
            bot.Say(ns, "Ping...");
            bot._pinged = Environment.TickCount;
        }
        #endregion Commands

        #region Events

        /// <summary>
        /// Event handler. Handles handshaking.
        /// </summary>
        public void evt_connect(Bot bot, dAmnPacket packet)
        {
            ConIO.Write("Connected to the server: " + bot.Endpoint());

            bot.Send(dAmnPackets.dAmnClient(0.3, Program.BotName, bot.Config.Owner));
        }

        /// <summary>
        /// Event handler. Handles pre-auth.
        /// </summary>
        public void evt_preauth(Bot bot, dAmnPacket packet)
        {
            ConIO.Write("Connected to dAmnServer version " + packet.Parameter);

            bot.Send(dAmnPackets.Login(bot.Config.Username, bot.Config.Authtoken));
        }

        /// <summary>
        /// Event handler. Handles login.
        /// </summary>
        public void evt_login(Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("Logging in as {0} [{1}]", packet.Parameter, packet.Arguments["e"]));

            if (packet.Arguments["e"] != "ok")
            {
                Program.Running = false;
            }
            else
            {
                // Uncomment this later, when we add a BDS extension.
                //bot.Join("chat:datashare");
                foreach (String channel in bot.Config.Channels)
                {
                    bot.Join(channel);
                }
            }
        }

        /// <summary>
        /// Event handler. Handles joins.
        /// </summary>
        public void evt_join(Bot bot, dAmnPacket packet)
        {
            if (packet.Arguments["e"] == "ok")
            {
                ConIO.Write(String.Format("** Joined [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
            }
            else
            {
                ConIO.Write(String.Format("** Failed to join [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
            }
        }

        /// <summary>
        /// Event handler. Handles parts.
        /// </summary>
        public void evt_part(Bot bot, dAmnPacket packet)
        {
            if (packet.Arguments["e"] == "ok")
            {
                // Change output depending on whether or not we have a reason
                if (packet.Arguments.ContainsKey("r"))
                {
                    ConIO.Write(String.Format("** Parted [{0}] ({1})", packet.Arguments["e"], packet.Arguments["r"]), Tools.FormatChat(packet.Parameter));
                    // If we parted with a reason, that means we disconnected or timed out!
                    bot.Reconnect();
                }
                else
                {
                    ConIO.Write(String.Format("** Parted [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to part [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
            }
        }

        /// <summary>
        /// Event handler. Handles properties.
        /// </summary>
        public void evt_property(Bot bot, dAmnPacket packet)
        {
            // We need to store these later!
            ConIO.Write(String.Format("*** Got {0}", packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles messages.
        /// </summary>
        public void evt_recv_msg(Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("<{0}> {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));

            // Pong!
            if (bot._pinged != 0 && packet.Body == "Ping..." && packet.Arguments["from"].ToLower() == bot.Config.Username.ToLower())
            {
                bot.Say(packet.Parameter, String.Format("Pong! {0}ms.", Environment.TickCount - bot._pinged));
                bot._pinged = 0;
            }

            // Check for commands!
            if (packet.Body.StartsWith(bot.Config.Trigger))
            {
                Events.CallCommand(packet.Body.Substring(bot.Config.Trigger.Length), packet);
            }
        }

        /// <summary>
        /// Event handler. Handles actions.
        /// </summary>
        public void evt_recv_action(Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("* {0} {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles pinging.
        /// </summary>
        public void evt_ping(Bot bot, dAmnPacket packet)
        {
            // Don't see a reason to write a packet object for this.
            bot.Send("pong\n\0");
        }

        #endregion Events
    }
}
