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
            String output = "<bcode>";

            bool verbose = (args.Length >= 2 && args[1] == "verbose");

            output += String.Format("&raquo; Data sent : {0}\n", Tools.FormatBytes(Program.bytes_sent, verbose));
            output += String.Format("&raquo; Data recv : {0}\n", Tools.FormatBytes(Program.bytes_received, verbose));
            output += String.Format("&raquo; Packets   : OUT: {0}\t\tIN: {1}\n", Program.packets_out, Program.packets_in);
            output += String.Format("&raquo; Queues    : OUT: {0}\t\tIN: {1}\n", bot.QueuedOut, bot.QueuedIn);
            output += "</bcode>";

            bot.Say(ns, output);
        }
    }
}

