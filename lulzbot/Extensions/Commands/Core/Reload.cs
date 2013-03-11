using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_reload (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            bot.Say(ns, "<b>&raquo; Reloading extensions...</b>");
            int start = Environment.TickCount;

            Events.ClearExternalEvents();
            Bot.Extensions.Load();

            bot.Say(ns, "<b>&raquo; Done!</b> Took " + Tools.FormatTime((int)(Environment.TickCount - start) / 1000));
        }
    }
}

