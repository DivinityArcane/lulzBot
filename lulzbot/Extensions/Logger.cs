using System;

namespace lulzbot.Extensions
{
    public class Logger
    {
        public Logger()
        {
            Events.AddEvent("log_msg", new Event(this, "handle_log_msg", "Handles logging messages."));
        }

        /// <summary>
        /// Handles logging messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="ns">namespace, i.e. #Botdom</param>
        /// <param name="msg">output message. i.e. "<Botdom> Hi there"</param>
        public void handle_log_msg(Bot bot, String ns, String msg)
        {
            String month = Tools.strftime("Y-%m %B");
            String day = Tools.strftime("%B %d %Y");
            String path = String.Format("./Storage/Logs/{0}/{1}/{2}.txt", ns, month, day);
            String content = String.Format("[{0}] {1}{2}", ns, msg, Environment.NewLine);
            Tools.WriteFile(path, content, true);
        }
    }
}
