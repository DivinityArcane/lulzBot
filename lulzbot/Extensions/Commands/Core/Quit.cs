using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_quit(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, String.Format("<b>&raquo; Quitting. [Uptime: {0}]</b>", Tools.FormatTime(bot.uptime)));
            bot.Quitting = true;
            Program.Running = false;
            Program.wait_event.Set();
            bot.Disconnect();
        }
    }
}

