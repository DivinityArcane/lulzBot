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
            // Log messages?
        }
    }
}
