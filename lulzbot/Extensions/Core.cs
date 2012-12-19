using lulzbot.Networking;
using System;
using System.Collections.Generic;

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
            Events.AddEvent("recv_join",    new Event(this, "evt_recv_join"));
            Events.AddEvent("recv_part",    new Event(this, "evt_recv_part"));
            Events.AddEvent("recv_privchg", new Event(this, "evt_recv_privchg"));
            Events.AddEvent("recv_kicked",  new Event(this, "evt_recv_kicked"));
            Events.AddEvent("recv_admin",   new Event(this, "evt_recv_admin"));
            Events.AddEvent("kicked",       new Event(this, "evt_kicked"));
            Events.AddEvent("disconnect",   new Event(this, "evt_disconnect"));
            Events.AddEvent("send",         new Event(this, "evt_send_error"));
            Events.AddEvent("kick",         new Event(this, "evt_kick_error"));
            Events.AddEvent("get",          new Event(this, "evt_get_error"));
            Events.AddEvent("set",          new Event(this, "evt_set_error"));
            Events.AddEvent("kill",         new Event(this, "evt_kill_error"));
            Events.AddEvent("ping",         new Event(this, "evt_ping"));

            // Again, simple format:
            // Events.AddCommand("command_name", cmd_obj);
            // Where cmd_obj is:
            //  new Command(this, "function_name", "Your_dA_username", "A simple help msg.", minimum_privs, "Description")
            // minimum_privs = minimum privilege level. 25 = guests, 50 = members, 75 = opers, 99 = admins, 100 = owner.
            Events.AddCommand("about",      new Command(this, "cmd_about", "DivinityArcane", "No help.", 25, "Displays information about the bot."));
            Events.AddCommand("ping",       new Command(this, "cmd_ping", "DivinityArcane", "No help.", 25, "Tests the latency between the bot and the server."));
            Events.AddCommand("commands",   new Command(this, "cmd_commands", "DivinityArcane", "No help", 25, "Displays commands available to the user."));
            Events.AddCommand("quit",       new Command(this, "cmd_quit", "DivinityArcane", "No help", 100, "Closes the bot down gracefully."));
        }

        #region Commands
        /// <summary>
        /// About command!
        /// </summary>
        public void cmd_about(Bot bot, String ns, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; {0} v{1}</b> - <i>\"Embrace the lulz. &trade;\"</i><br/>", Program.BotName, Program.Version);
            output += String.Format("<b>&raquo; Written by:</b> :devDivinityArcane:, :devOrrinFox:<br/><b>&raquo; Owned by:</b> :dev{0}:<br/>", bot.Config.Owner);
            output += String.Format("<b>&raquo; System:</b> {0}", Program.OS);

            bot.Say(ns, output);
        }

        /// <summary>
        /// Ping command!
        /// </summary>
        public void cmd_ping(Bot bot, String ns, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "Ping...");
            bot._pinged = Environment.TickCount;
        }

        /// <summary>
        /// Commands command!
        /// </summary>
        public void cmd_commands(Bot bot, String ns, String msg, String from, dAmnPacket packet)
        {
            // To be replaced later when the user system is added
            int my_privs = 25;

            List<String> commands = Events.GetAvailableCommands(my_privs);

            if (commands.Count <= 0)
            {
                bot.Say(ns, "<b>&raquo; No command available for :dev" + from + ":.");
            }
            else
            {
                bot.Say(ns, String.Format("<b>&raquo; {0} command(s) available for :dev{1}::<br/>&raquo;</b> {2}", commands.Count, from, String.Join(", ", commands)));
            }
        }

        /// <summary>
        /// Quit command!
        /// </summary>
        public void cmd_quit(Bot bot, String ns, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "<b>&raquo; Quitting.</b>");
            bot.Quitting = true;
            bot.Disconnect();
        }
        #endregion Commands

        #region Events

        /// <summary>
        /// Event handler. Handles handshaking.
        /// </summary>
        public void evt_connect(Bot bot, dAmnPacket packet)
        {
            if (Program.Debug)
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
                bot.Join("chat:datashare");
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
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

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
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            if (packet.Arguments["e"] == "ok")
            {
                // Change output depending on whether or not we have a reason
                if (packet.Arguments.ContainsKey("r"))
                {
                    ConIO.Write(String.Format("** Left [{0}] ({1})", packet.Arguments["e"], packet.Arguments["r"]), Tools.FormatChat(packet.Parameter));
                    // If we parted with a reason, that means we disconnected or timed out!
                    bot.Reconnect();
                }
                else
                {
                    ConIO.Write(String.Format("** Left [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to leave [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
            }
        }

        /// <summary>
        /// Event handler. Handles properties.
        /// </summary>
        public void evt_property(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            // We need to store these later!

            // Only output this in debug mode.
            if (Program.Debug)
                ConIO.Write(String.Format("*** Got {0}", packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles messages.
        /// </summary>
        public void evt_recv_msg(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("<{0}> {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));

            // Pong!
            if (bot._pinged != 0 && packet.Body == "Ping..." && packet.Arguments["from"].ToLower() == bot.Config.Username.ToLower())
            {
                bot.Say(packet.Parameter, String.Format("Pong! {0}ms.", Environment.TickCount - bot._pinged));
                bot._pinged = 0;
            }

            // Check for trigcheck, etc
            if (packet.Body.ToLower().StartsWith(bot.Config.Username.ToLower() + ": "))
            {
                String msg = packet.Body.Substring(bot.Config.Username.Length + 2);

                if (msg.ToLower() == "trigcheck")
                {
                    bot.Say(packet.Parameter, String.Format("{0}: My trigger is <b><code>{1}</code></b>", packet.Arguments["from"], bot.Config.Trigger.Replace("&", "&amp;")));
                }
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
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("* {0} {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles joins.
        /// </summary>
        public void evt_recv_join(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            // Due to the odd format of this packet, arguments are pushed to the body.
            packet.PullBodyArguments();

            if (packet.Arguments["realname"].Length > 0)
                ConIO.Write(String.Format("** {0}{1} ({2}) joined.", packet.Arguments["symbol"], packet.SubParameter, packet.Arguments["realname"]), Tools.FormatChat(packet.Parameter));
            else
                ConIO.Write(String.Format("** {0}{1} joined.", packet.Arguments["symbol"], packet.SubParameter), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles parts.
        /// </summary>
        public void evt_recv_part(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("** {0} left.", packet.SubParameter), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles privclass changes.
        /// </summary>
        public void evt_recv_privchg(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** {0} has been made a member of {1} by {2}", packet.SubParameter, packet.Arguments["pc"], packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles kicks.
        /// </summary>
        public void evt_recv_kicked(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            if (packet.Body.Length > 0)
                ConIO.Write(String.Format("*** {0} has been kicked by {1}: {2}", packet.SubParameter, packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
            else
                ConIO.Write(String.Format("*** {0} has been kicked by {1}", packet.SubParameter, packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles admin changes.
        /// </summary>
        public void evt_recv_admin(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            if (packet.SubParameter == "create" || packet.SubParameter == "update")
                ConIO.Write(String.Format("*** {0} {1}d privclass {2} with: {3}", packet.Arguments["by"], packet.SubParameter, packet.Arguments["name"], packet.Arguments["privs"]), Tools.FormatChat(packet.Parameter));
            else if (packet.SubParameter == "rename")
                ConIO.Write(String.Format("*** {0} renamed privclass {1} to {2}", packet.Arguments["by"], packet.Arguments["prev"], packet.Arguments["name"]), Tools.FormatChat(packet.Parameter));
            else if (packet.SubParameter == "move")
                ConIO.Write(String.Format("*** {0} moved all users of privclass {1} to {2}. {3} user(s) were affected", packet.Arguments["by"], packet.Arguments["prev"], packet.Arguments["name"], packet.Arguments["n"]), Tools.FormatChat(packet.Parameter));
            else if (packet.SubParameter == "remove")
                ConIO.Write(String.Format("*** {0} removed privclass {1}. {2} user(s) were affected", packet.Arguments["by"], packet.Arguments["name"], packet.Arguments["n"]), Tools.FormatChat(packet.Parameter));
            else if (packet.SubParameter == "privclass")
                ConIO.Write(String.Format("*** Failed to {0} privclass: {1}", packet.Arguments["p"], packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));

            // We don't need output for SHOW

        }

        /// <summary>
        /// Event handler. Handles the bot being kicked.
        /// </summary>
        public void evt_kicked(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            if (packet.Body.Length > 0)
                ConIO.Write(String.Format("*** Kicked by {0}: {1}", packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
            else
                ConIO.Write(String.Format("*** Kicked by {0}", packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));
            
            // Rejoin!
            if (bot.AutoReJoin)
                bot.Join(packet.Parameter);
        }

        /// <summary>
        /// Event handler. Handles disconnects.
        /// </summary>
        public void evt_disconnect(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** Disconnected [{0}]", packet.Arguments["e"]));

            // Add an override for a restart command later?
            if (bot.Quitting)
                bot.Close();
            else
                bot.Reconnect();
        }

        /// <summary>
        /// Event handler. Handles send errors.
        /// </summary>
        public void evt_send_error(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** Failed to send to {0} [{1}]", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            bot.Reconnect();
        }

        /// <summary>
        /// Event handler. Handles kick errors.
        /// </summary>
        public void evt_kick_error(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** Failed to kick {0} from {1} [{2}]", packet.Arguments["u"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            bot.Reconnect();
        }

        /// <summary>
        /// Event handler. Handles get errors.
        /// </summary>
        public void evt_get_error(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** Failed to get {0} in {1} [{2}]", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            bot.Reconnect();
        }

        /// <summary>
        /// Event handler. Handles set errors.
        /// </summary>
        public void evt_set_error(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            ConIO.Write(String.Format("*** Failed to set {0} in {1} [{2}]", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            bot.Reconnect();
        }

        /// <summary>
        /// Event handler. Handles kill errors.
        /// </summary>
        public void evt_kill_error(Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("*** Failed to kill {0} [{1}]", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));
            bot.Reconnect();
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
