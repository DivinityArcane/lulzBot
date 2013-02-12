using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_preauth(Bot bot, dAmnPacket packet)
        {
            ConIO.Write("Connected to dAmnServer version " + packet.Parameter);

            bot.Send(dAmnPackets.Login(bot.Config.Username, bot.Config.Authtoken));
        }
    }
}

