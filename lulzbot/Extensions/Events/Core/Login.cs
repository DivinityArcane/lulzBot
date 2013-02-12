using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_login(Bot bot, dAmnPacket packet)
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
    }
}

