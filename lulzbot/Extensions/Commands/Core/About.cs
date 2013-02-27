using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_about (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; I am a {0} v{1}, written by :devDivinityArcane: and I am owned by :dev{2}:</b><br/>", Program.BotName, Program.Version, bot.Config.Owner);
            //output += String.Format("<b>&raquo; Owned by:</b> :dev{0}:<br/>", bot.Config.Owner);
            //output += String.Format("<b>&raquo; System:</b> {0}<br/>", Program.OS);
            output += String.Format("<b>&raquo;</b> I've disconnected {0} time{1}, while I've been running for {2}<br/>", Program.Disconnects, Program.Disconnects == 1 ? "" : "s", Tools.FormatTime(bot.uptime));
            //output += String.Format("<b>&raquo; Disconnections:</b> {0}", Program.Disconnects);

            bot.Act(ns, output);
        }
    }
}

