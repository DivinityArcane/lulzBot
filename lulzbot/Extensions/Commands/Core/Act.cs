﻿using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_act(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 3)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}act #channel <i>msg</i>", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                lock (CommandChannels["send"])
                {
                    CommandChannels["send"].Add(ns);
                }

                bot.Act(args[1], msg.Substring(5 + args[1].Length));
            }
        }
    }
}

