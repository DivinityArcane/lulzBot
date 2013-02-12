using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_kick(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 3)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}kick #channel username <i>reason</i>", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                lock (CommandChannels["kick"])
                {
                    CommandChannels["kick"].Add(ns);
                }

                bot.Kick(args[1], args[2], "<b>"+from+"</b>"+(args.Length >= 4 ? ": "+msg.Substring(7+args[1].Length+args[2].Length) : ""));
            }
        }
    }
}

