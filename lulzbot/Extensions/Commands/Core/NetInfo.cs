using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_netinfo(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String output = String.Empty;

            bool verbose = (args.Length >= 2 && args[1] == "verbose");

            output += String.Format("<b>&raquo; Data sent:</b> {0}<br/>", Tools.FormatBytes(Program.bytes_sent, verbose));
            output += String.Format("<b>&raquo; Data recv:</b> {0}", Tools.FormatBytes(Program.bytes_received, verbose));

            bot.Say(ns, output);
        }
    }
}

