using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public class Logger
    {
        public struct LogConfig
        {
            public bool Enabled;
            public List<String> BlackList;
        };

        public static LogConfig Config;

        public Logger ()
        {
            Events.AddEvent("log_msg", new Event(this, "handle_log_msg", "Handles logging messages."));
            Events.AddCommand("logs", new Command(this, "cmd_logs", "DivinityArcane", 100, "Manages logging."));

            // Load saved data, if we can.
            Config = Storage.Load<LogConfig>("logging");

            if (Config.BlackList == null)
            {
                Config.Enabled = true;
                Config.BlackList = new List<String>();
                Save();
            }
        }

        public static void Save ()
        {
            Storage.Save("logging", Config);
        }

        /// <summary>
        /// Handles logging messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="ns">namespace, i.e. #Botdom</param>
        /// <param name="msg">output message. i.e. "<Botdom> Hi there"</param>
        public void handle_log_msg (Bot bot, String ns, String msg)
        {
            if (!Config.Enabled || Config.BlackList.Contains(Tools.FormatNamespace(ns.ToLower(), NamespaceFormat.Packet))) return;

            String day = Tools.strftime("%B %d %Y");
            String month = Tools.strftime("%Y-%m %B");
            String path = String.Format("Storage/Logs/{0}/{1}/{2}.txt", ns, month, day);
            String ts = Tools.strftime("%H:%M:%S %p");
            String content = String.Format("[{0}] {1}{2}", ts, msg, Environment.NewLine);

            Tools.WriteFile(path, content, true);
        }

        public void LogProperty (String ns, String prop, String content)
        {
            if (!Config.Enabled || Config.BlackList.Contains(Tools.FormatNamespace(ns.ToLower(), NamespaceFormat.Packet))) return;
            if (prop != "title" && prop != "topic") return;
            if (ns.ToLower() == "#datashare") return;

            Tools.WriteFile("Storage/Logs/" + ns + "/" + prop + ".txt", content);
        }

        public void cmd_logs (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage: {0}logs enable/disable{0}logs blacklist{0}logs blacklist add/remove #channel", "<br/> <b>&middot;</b> " + bot.Config.Trigger);

            if (args.Length == 1)
                bot.Say(ns, helpmsg);
            else
            {
                String cmd = args[1];

                if (cmd == "enable" || cmd == "disable")
                {
                    Config.Enabled = cmd == "enable";
                    bot.Say(ns, String.Format("<b>&raquo; Logging has been {0}d</b>", cmd));
                    Save();
                    return;
                }
                else if (args.Length == 2 && cmd == "blacklist")
                {
                    if (Config.BlackList.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; No channels have been blacklisted from logging.</b>");
                        return;
                    }

                    List<String> list = new List<String>();

                    foreach (String k in Config.BlackList)
                        list.Add(Tools.FormatNamespace(k, NamespaceFormat.Channel));

                    bot.Say(ns, String.Format("<b>&raquo; There's {0} blacklisted channel{1}:<br/> &middot; [</b>{2}<b>]</b>", list.Count, list.Count == 1 ? "" : "s", String.Join("<b>]</b>, <b>[</b>", list)));
                }
                else if (cmd == "blacklist" && args.Length == 4)
                {
                    String par = args[2];
                    String chan = args[3];

                    if (!chan.StartsWith("#"))
                    {
                        bot.Say(ns, "<b>&raquo; Invalid channel name. Valid channel names start with #</b>");
                        return;
                    }

                    chan = Tools.FormatNamespace(chan, NamespaceFormat.Packet).ToLower();

                    if (par == "add")
                    {
                        if (Config.BlackList.Contains(chan))
                        {
                            bot.Say(ns, "<b>&raquo; That channel is already in the blacklist.</b>");
                            return;
                        }

                        Config.BlackList.Add(chan);
                        Save();

                        bot.Say(ns, "<b>&raquo; Added channel to blacklist:</b> " + args[3]);
                    }
                    else if (par == "remove")
                    {
                        if (!Config.BlackList.Contains(chan))
                        {
                            bot.Say(ns, "<b>&raquo; That channel is not in the blacklist.</b>");
                            return;
                        }

                        Config.BlackList.Remove(chan);
                        Save();

                        bot.Say(ns, "<b>&raquo; Removed channel to blacklist:</b> " + args[3]);
                    }
                    else bot.Say(ns, helpmsg);
                }
                else bot.Say(ns, helpmsg);
            }
        }
    }
}
