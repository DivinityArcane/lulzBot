using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_disconnect(Bot bot, dAmnPacket packet)
        {
            ConIO.Write(String.Format("*** Disconnected [{0}]", packet.Arguments["e"]));

            // Add an override for a restart command later?
            if (bot.Quitting)
            {
                bot.Close();
            }
            else
                bot.Reconnect();
        }
    }
}
