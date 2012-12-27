using lulzbot.Networking;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public class Core
    {
        public static Dictionary<String, Types.ChatData> ChannelData = new Dictionary<String, Types.ChatData>();

        /// <summary>
        /// Keeps track of which channels certain commands were sent from.
        /// </summary>
        public static Dictionary<String, List<String>> CommandChannels = new Dictionary<String, List<String>>();

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
            Events.AddCommand("commands",   new Command(this, "cmd_commands", "DivinityArcane", "No help", 25, "Displays commands available to the user."));
            Events.AddCommand("event",      new Command(this, "cmd_event", "DivinityArcane", "{trig}event [info|hitcount|list]", 25, "Gets information on the events system."));
            Events.AddCommand("get",        new Command(this, "cmd_get", "DivinityArcane", "{trig}get #someChannel [title|topic|members|privclasses]", 50, "Gets the specified data for the specified channel."));
            Events.AddCommand("join",       new Command(this, "cmd_join", "DivinityArcane", "{trig}join #someChannel", 75, "Makes the bot join the specified channel."));
            Events.AddCommand("part",       new Command(this, "cmd_part", "DivinityArcane", "{trig}part #someChannel", 75, "Makes the bot leave the specified channel."));
            Events.AddCommand("ping",       new Command(this, "cmd_ping", "DivinityArcane", "No help.", 25, "Tests the latency between the bot and the server."));
            Events.AddCommand("quit",       new Command(this, "cmd_quit", "DivinityArcane", "No help", 100, "Closes the bot down gracefully."));
            Events.AddCommand("uptime",     new Command(this, "cmd_uptime", "DivinityArcane", "No help.", 25, "Returns how long the bot has been running."));

            // Initialize CommandsChannels
            String[] c_types = new String[] { "join", "part", "say", "set", "kick", "kill", "promote", "demote", "admin" };
            CommandChannels.Clear();

            foreach (String c_type in c_types)
                CommandChannels.Add(c_type, new List<String>());
        }

        #region Commands
        /// <summary>
        /// About command!
        /// </summary>
        public void cmd_about(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; {0} v{1}</b> - <i>\"Embrace the lulz. &trade;\"</i><br/>", Program.BotName, Program.Version);
            output += String.Format("<b>&raquo; Written by:</b> :devDivinityArcane:, :devOrrinFox:<br/><b>&raquo; Owned by:</b> :dev{0}:<br/>", bot.Config.Owner);
            output += String.Format("<b>&raquo; System:</b> {0}<br/>", Program.OS);
            output += String.Format("<b>&raquo; Uptime:</b> {0}", Tools.FormatTime(bot.uptime));

            bot.Say(ns, output);
        }

        /// <summary>
        /// Commands command!
        /// </summary>
        public void cmd_commands(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            // To be replaced later when the user system is added
            int my_privs = 25;

            if (from.ToLower() == bot.Config.Owner.ToLower())
                my_privs = 100;

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
        /// Event information command!
        /// </summary>
        public void cmd_event(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length == 1 || (args[1] != "hitcount" && args[1] != "list" && (args[1] != "info" || args.Length != 3)))
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage</b><br/>&raquo; {0}event hitcount<br/>&raquo; {0}event list<br/>&raquo; {0}event info [event]", bot.Config.Trigger));
            }
            else
            {
                if (args[1] == "hitcount")
                {
                    Dictionary<String, UInt32> hitcounts = Events.HitCounts;
                    List<String> keys = new List<String>(hitcounts.Keys);
                    uint total = 0;

                    String output = String.Empty;

                    keys.Sort();

                    foreach (String key in keys)
                    {
                        uint value = hitcounts[key];
                        if (value <= 0)
                            continue;
                        output += String.Format("\n&raquo; {0}: {1} hit{2}", key.PadRight(15, ' '), value, (value == 1 ? "" : "s"));
                        total += value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} event hits:</b><bcode>{1}</bcode><i>&raquo; Events with 0 hits are not displayed.</i>", total, output));
                }
                else if (args[1] == "list")
                {
                    List<String> keys = new List<String>(Events.GetEvents().Keys);

                    String output = String.Empty;

                    keys.Sort();

                    foreach (String key in keys)
                    {
                        output += String.Format("<br/>&raquo; <b>{0}</b>", key);
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} events:</b>{1}", keys.Count, output));
                }
                else if (args[1] == "info")
                {
                    Dictionary<String, List<Event>> events = Events.GetEvents();

                    if (!events.ContainsKey(args[2]))
                    {
                        bot.Say(ns, "<b>&raquo; That event is not valid.</b> Events are case sensitive.");
                        return;
                    }

                    String output = String.Empty;
                    int bound_count = 0;

                    foreach (Event evt in events[args[2]])
                    {
                        bound_count++;
                        output += String.Format("\nCallback {0}\n\tClass: {1}\n\tMethod: {2}\n\tDescription: {3}\n", bound_count, evt.Class.ToString(), evt.Method.Name, evt.Description); 
                    }

                    bot.Say(ns, String.Format("<b>&raquo; {0} callbacks bound to event '{1}':</b><bcode>{2}</bcode>", bound_count, args[2], output));
                }
            }
        }

        /// <summary>
        /// Get command!
        /// </summary>
        public void cmd_get(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 3)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}get #channel [title|topic|members|privclasses]", bot.Config.Trigger));
            }
            else
            {
                // We need it in chat:xxxx format
                String chan = Tools.FormatChat(args[1]).ToLower();
                String prop = args[2];

                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                if (prop != "title" && prop != "topic" && prop != "members" && prop != "privclasses")
                {
                    bot.Say(ns, "<b>&raquo; Invalid property!</b> Valid properties are title, topic, members, and privclasses.");
                    return;
                }

                if (!ChannelData.ContainsKey(chan))
                {
                    bot.Say(ns, "<b>&raquo; No data for that channel!</b>");
                    return;
                }

                lock (ChannelData[chan])
                {
                    Types.ChatData data = ChannelData[chan];

                    // Correct capitalization and #
                    String friendly_name = Tools.FormatChat(data.Name);

                    if (prop == "title")
                    {
                        if (data.Title.Length < 1)
                            bot.Say(ns, String.Format("<b>&raquo; Title for {0} is empty.</b>", friendly_name));
                        else
                            bot.Say(ns, String.Format("<b>&raquo; Title for {0}:</b><br/>{1}", friendly_name, data.Title));
                    }
                    else if (prop == "topic")
                    {
                        if (data.Topic.Length < 1)
                            bot.Say(ns, String.Format("<b>&raquo; Topic for {0} is empty.</b>", friendly_name));
                        else
                            bot.Say(ns, String.Format("<b>&raquo; Topic for {0}:</b><br/>{1}", friendly_name, data.Topic));
                    }
                    else if (prop == "members")
                    {
                        if (data.Members.Count < 1)
                            bot.Say(ns, String.Format("<b>&raquo; No members for {0}.</b>", friendly_name));
                        else
                        {
                            String members = String.Empty;

                            Dictionary<String, List<String>> ordered_list = new Dictionary<String, List<String>>();

                            foreach (Types.ChatMember member in data.Members.Values)
                            {
                                if (!ordered_list.ContainsKey(member.Privclass))
                                    ordered_list.Add(member.Privclass, new List<String>());

                                // We split the names to stop it from tabbing people
                                ordered_list[member.Privclass].Add(member.Name.Substring(0, 1) + "<i></i>" + member.Name.Substring(1));
                            }

                            foreach (Types.Privclass privclass in data.Privclasses.Values)
                            {
                                if (!ordered_list.ContainsKey(privclass.Name))
                                {
                                    members += String.Format("<br/><b>{0}</b>: None.", privclass.Name);
                                    continue;
                                }

                                ordered_list[privclass.Name].Sort();

                                members += String.Format("<br/><b>{0}</b>: <b>[</b>{1}<b>]</b>", privclass.Name, String.Join("<b>], [</b>", ordered_list[privclass.Name]));
                            }

                            bot.Say(ns, String.Format("<b>&raquo; {0} member(s) in {1}:</b>{2}", data.Members.Count, friendly_name, members));
                        }
                    }
                    else if (prop == "privclasses")
                    {
                        if (data.Privclasses.Count < 1)
                            bot.Say(ns, String.Format("<b>&raquo; No privclasses for {0}.</b>", friendly_name));
                        else
                        {
                            String privclasses = String.Empty;

                            foreach (Types.Privclass pc in data.Privclasses.Values)
                            {
                                privclasses += String.Format("<br/>&raquo; {0}: {1}", pc.Order, pc.Name);
                            }

                            bot.Say(ns, String.Format("<b>&raquo; Privclasses in {0}:</b>{1}", friendly_name, privclasses));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Join command!
        /// </summary>
        public void cmd_join(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}join #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                lock (CommandChannels["join"])
                {
                    CommandChannels["join"].Add(ns);
                }

                bot.Join(args[1]);
            }
        }

        /// <summary>
        /// Part command!
        /// </summary>
        public void cmd_part(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}part #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                lock (CommandChannels["part"])
                {
                    CommandChannels["part"].Add(ns);
                }

                bot.Part(args[1]);
            }
        }

        /// <summary>
        /// Ping command!
        /// </summary>
        public void cmd_ping(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "Ping...");
            bot._pinged = Environment.TickCount;
        }

        /// <summary>
        /// Quit command!
        /// </summary>
        public void cmd_quit(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, String.Format("<b>&raquo; Quitting. [Uptime: {0}]</b>", Tools.FormatTime(bot.uptime)));
            bot.Quitting = true;
            bot.Disconnect();
        }

        /// <summary>
        /// Uptime command!
        /// </summary>
        public void cmd_uptime(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String uptime = Tools.FormatTime(bot.uptime);
            bot.Say(ns, String.Format("<b>&raquo; Bot uptime:</b> {0}", uptime));
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
                Program.wait_event.Set();
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

                // Initialize channel data
                lock (ChannelData)
                {
                    if (!ChannelData.ContainsKey(packet.Parameter.ToLower()))
                    {
                        ChannelData.Add(packet.Parameter.ToLower(), new Types.ChatData());
                        ChannelData[packet.Parameter.ToLower()].Name = packet.Parameter;
                    }
                }

                lock (CommandChannels["join"])
                {
                    if (CommandChannels["join"].Count != 0)
                    {
                        String chan = CommandChannels["join"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Joined {0} [ok]</b>", Tools.FormatChat(packet.Parameter)));

                        CommandChannels["join"].RemoveAt(0);
                    }
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to join [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                lock (CommandChannels["join"])
                {
                    if (CommandChannels["join"].Count != 0)
                    {
                        String chan = CommandChannels["join"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Failed to join {0} [{1}]</b>", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));

                        CommandChannels["join"].RemoveAt(0);
                    }
                }
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
                    return;
                }
                else
                {
                    ConIO.Write(String.Format("** Left [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
                }

                // Remove channel data
                lock (ChannelData)
                {
                    if (ChannelData.ContainsKey(packet.Parameter.ToLower()))
                        ChannelData.Remove(packet.Parameter.ToLower());
                }

                lock (CommandChannels["part"])
                {
                    if (CommandChannels["part"].Count != 0)
                    {
                        String chan = CommandChannels["part"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Left {0} [ok]</b>", Tools.FormatChat(packet.Parameter)));

                        CommandChannels["part"].RemoveAt(0);
                    }
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to leave [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                lock (CommandChannels["part"])
                {
                    if (CommandChannels["part"].Count != 0)
                    {
                        String chan = CommandChannels["part"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Failed to leave {0} [{1}]</b>", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));

                        CommandChannels["part"].RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// Event handler. Handles properties.
        /// </summary>
        public void evt_property(Bot bot, dAmnPacket packet)
        {
            // Only output this in debug mode.
            if (Program.Debug && packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("*** Got {0}", packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));

            // Store data
            String ns = packet.Parameter;
            String type = packet.Arguments["p"];

            lock (ChannelData)
            {
                if (!ChannelData.ContainsKey(ns.ToLower()))
                {
                    ChannelData.Add(ns.ToLower(), new Types.ChatData());
                    ChannelData[ns.ToLower()].Name = ns;
                }
            }

            lock (ChannelData[ns.ToLower()])
            {
                if (type == "topic")
                    ChannelData[ns.ToLower()].Topic = packet.Body;
                else if (type == "title")
                    ChannelData[ns.ToLower()].Title = packet.Body;
                else if (type == "privclasses")
                {
                    // Ensure we don't run into duplicates.
                    ChannelData[ns.ToLower()].Privclasses.Clear();

                    foreach (String pc in packet.Body.Split('\n'))
                    {
                        if (pc.Length < 3 || !pc.Contains(":"))
                            continue;

                        Types.Privclass privclass = new Types.Privclass();

                        privclass.Order = Convert.ToByte(pc.Split(':')[0]);
                        privclass.Name = pc.Split(':')[1];

                        ChannelData[ns.ToLower()].Privclasses.Add(privclass.Name.ToLower(), privclass);
                    }
                }
                else if (type == "members")
                {
                    // Ensure we don't run into duplicates.
                    ChannelData[ns.ToLower()].Members.Clear();

                    String[] data = packet.Body.Split('\n');

                    for (int x = 0; x < data.Length; x++)
                    {
                        if (data[x].Length < 3 || !data[x].StartsWith("member") || x + 6 >= data.Length)
                            continue;

                        Types.ChatMember member = new Types.ChatMember();

                        member.Name = data[x].Substring(7);

                        // We get duplicates on multiple connections.
                        if (ChannelData[ns.ToLower()].Members.ContainsKey(member.Name.ToLower()))
                            continue;

                        member.Privclass = data[++x].Substring(3);

                        // We don't store the user icon. It's useless to us. Increment x anyway.
                        ++x;

                        member.Symbol = data[++x].Substring(7);
                        member.RealName = data[++x].Substring(9);
                        member.TypeName = data[++x].Substring(9);
                        member.GPC = data[++x].Substring(4);

                        ChannelData[ns.ToLower()].Members.Add(member.Name.ToLower(), member);

                        // Increment x for the blank line.
                        x++;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler. Handles messages.
        /// </summary>
        public void evt_recv_msg(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
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
                String cmd_name = String.Empty;

                if (packet.Body.Contains(" "))
                    cmd_name = packet.Body.Substring(bot.Config.Trigger.Length, packet.Body.IndexOf(' ') - bot.Config.Trigger.Length);
                else
                    cmd_name = packet.Body.Substring(bot.Config.Trigger.Length);

                Events.CallCommand(cmd_name, packet);
            }
        }

        /// <summary>
        /// Event handler. Handles actions.
        /// </summary>
        public void evt_recv_action(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("* {0} {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));
        }

        /// <summary>
        /// Event handler. Handles joins.
        /// </summary>
        public void evt_recv_join(Bot bot, dAmnPacket packet)
        {

            // Due to the odd format of this packet, arguments are pushed to the body.
            packet.PullBodyArguments();

            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("** {0}{1} joined. [{2}]", packet.Arguments["symbol"], packet.SubParameter, packet.Arguments["pc"]), Tools.FormatChat(packet.Parameter));

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (!ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    Types.ChatMember member = new Types.ChatMember();

                    member.Name = packet.SubParameter;
                    member.Privclass = packet.Arguments["pc"];
                    member.RealName = packet.Arguments["realname"];
                    member.TypeName = packet.Arguments["typename"];
                    member.Symbol = packet.Arguments["symbol"];
                    member.GPC = packet.Arguments["gpc"];

                    ChannelData[packet.Parameter.ToLower()].Members.Add(member.Name.ToLower(), member);
                }
            }
        }

        /// <summary>
        /// Event handler. Handles parts.
        /// </summary>
        public void evt_recv_part(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                if (packet.Arguments.ContainsKey("r"))
                    ConIO.Write(String.Format("** {0} left. [{1}]", packet.SubParameter, packet.Arguments["r"]), Tools.FormatChat(packet.Parameter));
                else
                    ConIO.Write(String.Format("** {0} left.", packet.SubParameter), Tools.FormatChat(packet.Parameter));

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    ChannelData[packet.Parameter.ToLower()].Members.Remove(packet.SubParameter.ToLower());
                }
            }
        }

        /// <summary>
        /// Event handler. Handles privclass changes.
        /// </summary>
        public void evt_recv_privchg(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("*** {0} has been made a member of {1} by {2}", packet.SubParameter, packet.Arguments["pc"], packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].Privclass = packet.Arguments["pc"];
                }
            }
        }

        /// <summary>
        /// Event handler. Handles kicks.
        /// </summary>
        public void evt_recv_kicked(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                if (packet.Body.Length > 0)
                    ConIO.Write(String.Format("*** {0} has been kicked by {1}: {2}", packet.SubParameter, packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
                else
                    ConIO.Write(String.Format("*** {0} has been kicked by {1}", packet.SubParameter, packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    ChannelData[packet.Parameter.ToLower()].Members.Remove(packet.SubParameter.ToLower());
                }
            }
        }

        /// <summary>
        /// Event handler. Handles admin changes.
        /// </summary>
        public void evt_recv_admin(Bot bot, dAmnPacket packet)
        {
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (packet.SubParameter == "create")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** {0} created privclass {1} with: {2}", packet.Arguments["by"], packet.Arguments["name"], packet.Arguments["privs"]), Tools.FormatChat(packet.Parameter));

                    // Update channel data
                    if (!ChannelData[packet.Parameter.ToLower()].Privclasses.ContainsKey(packet.Arguments["name"].ToLower()))
                    {
                        Types.Privclass privclass = new Types.Privclass();
                        privclass.Name = packet.Arguments["name"];

                        // Gotta extract the order!
                        int order_pos = packet.Arguments["privs"].IndexOf("order=") + 6;
                        int sp_pos = packet.Arguments["privs"].IndexOf(' ', order_pos);

                        // No space? It was only the order then
                        if (sp_pos == -1)
                            sp_pos = packet.Arguments["privs"].Length - (order_pos - 6);

                        privclass.Order = Convert.ToByte(packet.Arguments["privs"].Substring(order_pos, sp_pos - order_pos));

                        ChannelData[packet.Parameter.ToLower()].Privclasses.Add(privclass.Name.ToLower(), privclass);
                    }
                }
                else if (packet.SubParameter == "update")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** {0} created privclass {1} with: {2}", packet.Arguments["by"], packet.Arguments["name"], packet.Arguments["privs"]), Tools.FormatChat(packet.Parameter));

                    // Update channel data
                    if (ChannelData[packet.Parameter.ToLower()].Privclasses.ContainsKey(packet.Arguments["name"].ToLower()))
                    {
                        // Gotta extract the order!
                        int order_pos = packet.Arguments["privs"].IndexOf("order=") + 6;
                        int sp_pos = packet.Arguments["privs"].IndexOf(' ', order_pos);

                        // No space? It was only the order then
                        if (sp_pos == -1)
                            sp_pos = packet.Arguments["privs"].Length - (order_pos - 6);

                        ChannelData[packet.Parameter.ToLower()].Privclasses[packet.Arguments["name"].ToLower()].Order = Convert.ToByte(packet.Arguments["privs"].Substring(order_pos, sp_pos - order_pos));
                    }
                }
                else if (packet.SubParameter == "rename")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** {0} renamed privclass {1} to {2}", packet.Arguments["by"], packet.Arguments["prev"], packet.Arguments["name"]), Tools.FormatChat(packet.Parameter));

                    // Update channel data
                    if (ChannelData[packet.Parameter.ToLower()].Privclasses.ContainsKey(packet.Arguments["prev"].ToLower()))
                    {
                        Types.Privclass privclass = ChannelData[packet.Parameter.ToLower()].Privclasses[packet.Arguments["prev"].ToLower()];

                        privclass.Name = packet.Arguments["name"];

                        ChannelData[packet.Parameter.ToLower()].Privclasses.Add(privclass.Name.ToLower(), privclass);
                        ChannelData[packet.Parameter.ToLower()].Privclasses.Remove(packet.Arguments["prev"].ToLower());
                    }
                }
                else if (packet.SubParameter == "move")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** {0} moved all users of privclass {1} to {2}. {3} user(s) were affected", packet.Arguments["by"], packet.Arguments["prev"], packet.Arguments["name"], packet.Arguments["n"]), Tools.FormatChat(packet.Parameter));
                }
                else if (packet.SubParameter == "remove")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** {0} removed privclass {1}. {2} user(s) were affected", packet.Arguments["by"], packet.Arguments["name"], packet.Arguments["n"]), Tools.FormatChat(packet.Parameter));

                    // Update channel data
                    if (ChannelData[packet.Parameter.ToLower()].Privclasses.ContainsKey(packet.Arguments["name"].ToLower()))
                    {
                        ChannelData[packet.Parameter.ToLower()].Privclasses.Remove(packet.Arguments["name"].ToLower());
                    }
                }
                else if (packet.SubParameter == "privclass")
                {
                    // Don't display DataShare messages.
                    if (packet.Parameter.ToLower() != "chat:datashare")
                        ConIO.Write(String.Format("*** Failed to {0} privclass: {1}", packet.Arguments["p"], packet.Arguments["p"]), Tools.FormatChat(packet.Parameter));
                }
            }

            // We don't need output for SHOW
        }

        /// <summary>
        /// Event handler. Handles the bot being kicked.
        /// </summary>
        public void evt_kicked(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
            {
                if (packet.Body.Length > 0)
                    ConIO.Write(String.Format("*** Kicked by {0}: {1}", packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
                else
                    ConIO.Write(String.Format("*** Kicked by {0}", packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));
            }

            // In the event that we cannot (or will not) rejoin, remove channel data.
            lock (ChannelData)
            {
                if (ChannelData.ContainsKey(packet.Parameter.ToLower()))
                    ChannelData.Remove(packet.Parameter.ToLower());
            }
            
            // Rejoin!
            if (bot.AutoReJoin)
                bot.Join(packet.Parameter);
        }

        /// <summary>
        /// Event handler. Handles disconnects.
        /// </summary>
        public void evt_disconnect(Bot bot, dAmnPacket packet)
        {
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
