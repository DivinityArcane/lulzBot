using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_uptime(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String uptime = Tools.FormatTime(bot.uptime);
            bot.Say(ns, String.Format("<b>&raquo; Bot uptime:</b> {0}", uptime));
        }
    }
}

