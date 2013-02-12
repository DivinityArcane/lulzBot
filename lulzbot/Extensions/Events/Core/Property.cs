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
            }
        }
    }
}

