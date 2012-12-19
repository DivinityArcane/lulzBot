using lulzbot.Networking;
using System;

namespace lulzbot.Extensions
{
    public class BDS
    {
        private static String police_bot = "botdom";

        public BDS()
        {
            Events.AddEvent("recv_msg", new Event(this, "ParseBDS", "Parses BDS messages."));
        }

        /// <summary>
        /// Parses BDS messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="packet">Packet object</param>
        public void ParseBDS(Bot bot, dAmnPacket packet)
        {
            // Not from DS? Ignore it.
            if (packet.Parameter.ToLower() != "chat:datashare")
                return;

            // Doesn't contain segments? Ignore it.
            if (!packet.Body.Contains(":"))
                return;

            String msg = packet.Body;
            String[] bits = msg.ToLower().Split(':');
            String ns = packet.Parameter;
            String from = packet.Arguments["from"];
            String username = bot.Config.Username;
            String trigger = bot.Config.Trigger;
            String owner = bot.Config.Owner;

            if (bits[0] == "bds")
            {
                if (bits.Length >= 3 && bits[1] == "botcheck")
                {
                    if (bits[2] == "all" || (bits.Length >= 4 && bits[2] == "direct" && bits[3] == username.ToLower()))
                    {
                        // If it's not the police bot, return.
                        // Replace this with a privclass check later!
                        if (from.ToLower() != police_bot)
                            return;

                        String hashkey = Tools.md5((trigger + from + username).ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/0.3,{4},{5}", from, owner, Program.BotName, Program.Version, hashkey, trigger));
                    }
                }
                else if (bits.Length >= 4 && bits[1] == "botdef")
                {
                    if (bits[2] == "request" && bits[3] == username.ToLower())
                    {
                        // If it's not the police bot, return.
                        // Replace this with a privclass check later!
                        if (from.ToLower() != police_bot)
                            return;

                        String hashkey = Tools.md5((from + Program.BotName + "DivinityArcane;OrrinFox").ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTDEF:RESPONSE:{0},{1},{2},{3},{4},{5}", from, Program.BotName, "C#", "DivinityArcane;OrrinFox", "http://botdom.com/wiki/User:Kyogo/lulzBot", hashkey));
                    }
                }
            }
        }
    }
}
