using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_commands(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            List<String> commands = Events.GetAvailableCommands(Users.GetPrivs(from));

            if (commands.Count <= 0)
            {
                bot.Say(ns, "<b>&raquo; No command available for :dev" + from + ":.");
            }
            else
            {
                bot.Say(ns, String.Format("<b>&raquo; {0} command{1} available for :dev{2}::<br/>&raquo;</b> {3}", commands.Count, (commands.Count == 1 ? "" : "s"), from, String.Join(", ", commands)));
            }
        }
    }
}

