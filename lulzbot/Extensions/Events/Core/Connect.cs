using lulzbot.Networking;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void evt_connect (Bot bot, dAmnPacket packet)
        {
            if (Program.Debug)
                ConIO.Write("Connected to the server: " + bot.Endpoint());

            bot.Send(dAmnPackets.dAmnClient(0.3, Program.BotName, bot.Config.Owner));
        }
    }
}

