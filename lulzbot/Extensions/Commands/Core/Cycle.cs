using System;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_cycle (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length != 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}cycle #channel", bot.Config.Trigger));
            }
            else
            {
                if (!args[1].StartsWith("#"))
                {
                    bot.Say(ns, "<b>&raquo; Invalid channel!</b> Channels should start with a #");
                    return;
                }

                String cpns = Tools.FormatNamespace(args[1], Types.NamespaceFormat.Packet).ToLower();

                if (!Core.ChannelData.ContainsKey(cpns))
                {
                    bot.Say(ns, "<b>&raquo; It doesn't look like I'm in that channel.</b>");
                    return;
                }

                lock (CommandChannels["part"])
                {
                    CommandChannels["part"].Add(ns);
                }

                lock (CommandChannels["join"])
                {
                    CommandChannels["join"].Add(ns);
                }

                bot.Part(cpns);
                bot.Join(cpns);
            }
        }
    }
}

