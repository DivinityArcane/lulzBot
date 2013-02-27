using lulzbot.Types;
using System;

namespace lulzbot.Extensions
{
    public class Logger
    {
        public Logger ()
        {
            Events.AddEvent("log_msg", new Event(this, "handle_log_msg", "Handles logging messages."));
        }

        /// <summary>
        /// Handles logging messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="ns">namespace, i.e. #Botdom</param>
        /// <param name="msg">output message. i.e. "<Botdom> Hi there"</param>
        public void handle_log_msg (Bot bot, String ns, String msg)
        {
            String day = Tools.strftime("%B %d %Y");
            String month = Tools.strftime("%Y-%m %B");
            String path = String.Format("Storage/Logs/{0}/{1}/{2}.txt", ns, month, day);
            String ts = Tools.strftime("%H:%M:%S %p");
            String content = String.Format("[{0}] {1}{2}", ts, msg, Environment.NewLine);
            Tools.WriteFile(path, content, true);
        }

        public void LogProperty (String ns, String prop, String content)
        {
            if (prop != "title" && prop != "topic") return;
            if (ns.ToLower() == "#datashare") return;

            Tools.WriteFile("Storage/Logs/" + ns + "/" + prop + ".txt", content);
        }
    }
}
