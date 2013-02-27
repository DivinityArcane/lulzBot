using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_recv_msg (Bot bot, dAmnPacket packet)
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

        public static void evt_recv_action (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("* {0} {1}", packet.Arguments["from"], packet.Body), Tools.FormatChat(packet.Parameter));
        }

        public static void evt_recv_join (Bot bot, dAmnPacket packet)
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
                    member.ConnectionCount = 1;

                    ChannelData[packet.Parameter.ToLower()].Members.Add(member.Name.ToLower(), member);
                }
                else
                {
                    ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].ConnectionCount++;
                }
            }
        }

        public static void evt_recv_part (Bot bot, dAmnPacket packet)
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
                    ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].ConnectionCount--;

                    if (ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].ConnectionCount <= 0)
                        ChannelData[packet.Parameter.ToLower()].Members.Remove(packet.SubParameter.ToLower());
                }
            }
        }

        public static void evt_recv_privchg (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                ConIO.Write(String.Format("*** {0} has been made a member of {1} by {2}", packet.SubParameter, packet.Arguments["pc"], packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));

            if (packet.Arguments["by"].ToLower() == bot.Config.Username.ToLower())
            {
                lock (CommandChannels["send"])
                {
                    if (CommandChannels["send"].Count > 0)
                    {
                        CommandChannels["send"].RemoveAt(0);
                        // No need to display this.
                    }
                }
            }

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].Privclass = packet.Arguments["pc"];
                }
            }
        }

        public static void evt_recv_kicked (Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() != "chat:datashare")
                if (packet.Body.Length > 0)
                    ConIO.Write(String.Format("*** {0} has been kicked by {1}: {2}", packet.SubParameter, packet.Arguments["by"], packet.Body), Tools.FormatChat(packet.Parameter));
                else
                    ConIO.Write(String.Format("*** {0} has been kicked by {1}", packet.SubParameter, packet.Arguments["by"]), Tools.FormatChat(packet.Parameter));

            if (packet.Arguments["by"].ToLower() == bot.Config.Username.ToLower())
            {
                lock (CommandChannels["send"])
                {
                    if (CommandChannels["send"].Count > 0)
                    {
                        CommandChannels["send"].RemoveAt(0);
                        // No need to display this.
                    }
                }
            }

            // Update channel data
            lock (ChannelData[packet.Parameter.ToLower()])
            {
                if (ChannelData[packet.Parameter.ToLower()].Members.ContainsKey(packet.SubParameter.ToLower()))
                {
                    ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].ConnectionCount--;

                    if (ChannelData[packet.Parameter.ToLower()].Members[packet.SubParameter.ToLower()].ConnectionCount <= 0)
                        ChannelData[packet.Parameter.ToLower()].Members.Remove(packet.SubParameter.ToLower());
                }
            }
        }

        public static void evt_recv_admin (Bot bot, dAmnPacket packet)
        {
            String cmdchan = String.Empty;
            if (packet.Arguments.ContainsKey("by") && packet.Arguments["by"].ToLower() == bot.Config.Username.ToLower())
            {
                lock (CommandChannels["send"])
                {
                    if (CommandChannels["send"].Count > 0)
                    {
                        cmdchan = CommandChannels["send"][0];
                        CommandChannels["send"].RemoveAt(0);
                        // No need to display this.
                    }
                }
            }

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
                        int order_pos = packet.Arguments["privs"].IndexOf("order=");

                        if (order_pos == -1)
                            return;

                        order_pos += 6;
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
                        ConIO.Write(String.Format("*** Failed to {0} privclass: {1}", packet.Arguments["p"], packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                    lock (CommandChannels["send"])
                    {
                        if (CommandChannels["send"].Count > 0)
                        {
                            cmdchan = CommandChannels["send"][0];
                            CommandChannels["send"].RemoveAt(0);
                        }
                    }

                    if (cmdchan != String.Empty)
                        bot.Say(cmdchan, String.Format("<b>&raquo; Failed to {0} privclass:</b> {1}<br/><br/><b>Command was:</b> <bcode>'{2}'</bcode>", packet.Arguments["p"], packet.Arguments["e"], packet.Body));
                }
                else if (packet.SubParameter == "show")
                {
                    lock (CommandChannels["send"])
                    {
                        if (CommandChannels["send"].Count > 0)
                        {
                            cmdchan = CommandChannels["send"][0];
                            CommandChannels["send"].RemoveAt(0);
                        }
                    }

                    Events.CallEvent("evt_recv_admin_show", packet);

                    if (cmdchan != String.Empty)
                        bot.Say(cmdchan, String.Format("<b>&raquo; Showing {0} of {1}:</b><br/><code>'{2}'</code>", packet.Arguments["p"], Tools.FormatChat(packet.Parameter), packet.Body));
                }
            }
        }
    }
}

