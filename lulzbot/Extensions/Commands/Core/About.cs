using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_about(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            output += String.Format("<b>&raquo; {0} v{1} by :devDivinityArcane:, with collaboration from :devOrrinFox:</b><br/>", Program.BotName, Program.Version);
            output += String.Format("<b>&raquo; Owned by:</b> :dev{0}:<br/>", bot.Config.Owner);
            output += String.Format("<b>&raquo; System:</b> {0}<br/>", Program.OS);
            output += String.Format("<b>&raquo; Uptime:</b> {0}<br/>", Tools.FormatTime(bot.uptime));
            output += String.Format("<b>&raquo; Disconnections:</b> {0}", Program.Disconnects);

            bot.Say(ns, output);
        }
    }
}

