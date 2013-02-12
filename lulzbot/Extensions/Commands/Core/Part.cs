using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_part(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}part #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                lock (CommandChannels["part"])
                {
                    CommandChannels["part"].Add(ns);
                }

                bot.Part(args[1]);
            }
        }
    }
}
