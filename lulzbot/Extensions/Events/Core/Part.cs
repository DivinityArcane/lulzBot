using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_part(Bot bot, dAmnPacket packet)
        {
            // Don't display DataShare messages.
            if (packet.Parameter.ToLower() == "chat:datashare") return;

            if (packet.Arguments["e"] == "ok")
            {
                // Change output depending on whether or not we have a reason
                if (packet.Arguments.ContainsKey("r"))
                {
                    ConIO.Write(String.Format("** Left [{0}] ({1})", packet.Arguments["e"], packet.Arguments["r"]), Tools.FormatChat(packet.Parameter));
                    // If we parted with a reason, that means we disconnected or timed out!
                    bot.Reconnect();
                    return;
                }
                else
                {
                    ConIO.Write(String.Format("** Left [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));
                }

                // Remove channel data
                lock (ChannelData)
                {
                    if (ChannelData.ContainsKey(packet.Parameter.ToLower()))
                        ChannelData.Remove(packet.Parameter.ToLower());
                }

                lock (CommandChannels["part"])
                {
                    if (CommandChannels["part"].Count != 0)
                    {
                        String chan = CommandChannels["part"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Left {0} [ok]</b>", Tools.FormatChat(packet.Parameter)));

                        CommandChannels["part"].RemoveAt(0);
                    }
                }
            }
            else
            {
                ConIO.Write(String.Format("** Failed to leave [{0}]", packet.Arguments["e"]), Tools.FormatChat(packet.Parameter));

                lock (CommandChannels["part"])
                {
                    if (CommandChannels["part"].Count != 0)
                    {
                        String chan = CommandChannels["part"][0];

                        bot.Say(chan, String.Format("<b>&raquo; Failed to leave {0} [{1}]</b>", Tools.FormatChat(packet.Parameter), packet.Arguments["e"]));

                        CommandChannels["part"].RemoveAt(0);
                    }
                }
            }
        }
    }
}
