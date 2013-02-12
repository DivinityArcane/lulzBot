using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_set(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 4)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}set #channel [title|topic] [content]", bot.Config.Trigger));
            }
            else
            {
                // We need it in chat:xxxx format
                String chan = Tools.FormatChat(args[1]).ToLower();
                String prop = args[2];
                String body = msg.Substring(args[1].Length + args[2].Length + 5);

                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                if (prop != "title" && prop != "topic")
                {
                    bot.Say(ns, "<b>&raquo; Invalid property!</b> Valid properties are title and topic.");
                    return;
                }

                lock (CommandChannels["set"])
                {
                    CommandChannels["set"].Add(ns);

                    bot.Send(dAmnPackets.Set(chan, prop, body));
                }
            }
        }
    }
}

