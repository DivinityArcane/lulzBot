using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_property(Bot bot, dAmnPacket packet)
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
                        {
                            ChannelData[ns.ToLower()].Members[member.Name.ToLower()].ConnectionCount++;
                            continue;
                        }

                        member.Privclass = data[++x].Substring(3);

                        // We don't store the user icon. It's useless to us. Increment x anyway.
                        ++x;

                        member.Symbol = data[++x].Substring(7);
                        member.RealName = data[++x].Substring(9);
                        member.TypeName = data[++x].Substring(9);
                        member.GPC = data[++x].Substring(4);
                        member.ConnectionCount = 1;

                        ChannelData[ns.ToLower()].Members.Add(member.Name.ToLower(), member);

                        // Increment x for the blank line.
                        x++;
                    }
                }
                else if (type == "info")
                {
                    lock (CommandChannels["whois"])
                    {
                        if (CommandChannels["whois"].Count > 0)
                        {
                            String chan = CommandChannels["whois"][0];
                            CommandChannels["whois"].RemoveAt(0);

                            WhoisData wd = new WhoisData();

                            String[] data = packet.Body.Split(new char[] { '\n' });

                            // Don't parse what we don't need!
                            // Icon is 0
                            wd.Name     = packet.Parameter.Substring(6);
                            //wd.Symbol   = data[1].Substring(7);
                            wd.RealName = data[2].Substring(9);
                            //wd.TypeName = data[3].Substring(9);
                            wd.GPC      = data[4].Substring(4);

                            int conID   = 0;
                            wd.Connections.Add(new WhoisConnection());

                            for (int i = 7; i < data.Length; i++)
                            {
                                if (data[i] == "conn")
                                {
                                    conID++;
                                    wd.Connections.Add(new WhoisConnection() { ConnectionID = conID });
                                }
                                else if (data[i].StartsWith("online="))
                                    int.TryParse(data[i].Substring(7), out wd.Connections[conID].Online);
                                else if (data[i].StartsWith("idle="))
                                    int.TryParse(data[i].Substring(5), out wd.Connections[conID].Idle);
                                else if (data[i].StartsWith("ns ") && data[i] != "ns chat:DataShare")
                                    wd.Connections[conID].Channels.Add("#" + data[i].Substring(8));
                            }

                            String output = String.Format("<b>&raquo;</b> :icon{0}: :dev{0}:<br/><br/>", wd.Name);

                            output += String.Format("<i>{0}</i><br/>{1}", wd.RealName, wd.GPC == "guest" ? "" : "<b>dAmn " + wd.GPC + "</b><br/>");

                            foreach (WhoisConnection wc in wd.Connections)
                            {
                                wc.Channels.Sort();
                                output += String.Format("<br/><b>&raquo; Connection #{0}</b><br/> <b>&middot; Online:</b> {1}<br/> <b>&middot; Idle:</b> {2}<br/> <b>&middot; Channels:</b> <b>[</b>{3}<b>]</b><br/>", 
                                    wc.ConnectionID + 1, Tools.FormatTime(wc.Online), Tools.FormatTime(wc.Idle), String.Join("<b>]</b>, <b>[</b>", wc.Channels));
                            }

                            bot.Say(chan, output);
                        }
                    }
                }
            }
        }
    }
}

