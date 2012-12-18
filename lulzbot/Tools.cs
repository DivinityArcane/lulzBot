using System;

namespace lulzbot
{
    public class Tools
    {
        /// <summary>
        /// Format a chat namespace.
        /// </summary>
        /// <param name="channel">Namespace to format</param>
        /// <returns>Formatted namespace</returns>
        public static String FormatChat(String channel)
        {
            // This could arguably be better. Thinking of changing how it works alltogether. 
            if (channel.StartsWith("chat:"))
            {
                return "#" + channel.Substring(5);
            }
            else if (channel.StartsWith("#"))
            {
                return "chat:" + channel.Substring(1);
            }
            else if (channel.StartsWith("login:"))
            {
                return channel.Substring(6);
            }
            else
            {
                return channel;
            }
        }
    }
}
