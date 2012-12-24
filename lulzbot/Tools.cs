using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace lulzbot
{
    /// <summary>
    /// Various tools for use throughout the bot.
    /// </summary>
    public class Tools
    {
        // This will keep track of how many arguments each tablump uses.
        private static Dictionary<String, int> lump_arg_count = new Dictionary<string,int>();


        /// <summary>
        /// Initialize the tablumps lists
        /// </summary>
        public static void InitLumps()
        {
            lump_arg_count.Clear();

            // Zero args
            foreach (String lump in new List<String>() {
                // Opening tags
                "b","s","i","u","p","li","ul","ol","br","sup","sub","code","bcode",
                // Closing tags
                "/b","/i","/u","/s","/p","/li","/ul","/ol","/br","/sup","/sub","/code","/bcode","/iframe","/embed","/a","/acro","/abbr"})
            {
                lump_arg_count.Add(lump, 0);
            }

            // One arg
            foreach (String lump in new List<String>() {"abbr","acro"})
            {
                lump_arg_count.Add(lump, 1);
            }

            // Two args
            foreach (String lump in new List<String>() {"a","dev","avatar","link"})
            {
                // Technically, link is two OR three. But we'll deal with that.
                lump_arg_count.Add(lump, 2);
            }

            // Three args
            foreach (String lump in new List<String>() {"img","iframe","embed"})
            {
                lump_arg_count.Add(lump, 3);
            }

            // Four args
            // None

            // Five args
            foreach (String lump in new List<String>() {"emote"})
            {
                lump_arg_count.Add(lump, 5);
            }

            // Six args
            // None

            // Seven args
            foreach (String lump in new List<String>() {"thumb"})
            {
                lump_arg_count.Add(lump, 7);
            }
        }

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

        public static String ParseEntities(String message)
        {
            // Doesn't need to be parsed?
            if (!message.Contains("&"))
                return message;

            String parsed = message;

            // The basics
            parsed = parsed.Replace("&lt;", "<");
            parsed = parsed.Replace("&gt;", ">");
            
            //message = message.Replace("&amp;", "&");
            return parsed;
        }

        /// <summary>
        /// Parse tablumps in a message.
        /// </summary>
        /// <param name="message">Unparsed message</param>
        /// <returns>Parsed message</returns>
        public static String ParseTablumps(String message)
        {
            // Do the basics to get certains & out of our way.
            message = ParseEntities(message);

            // If there's no ampersand or tab, there's no tablumps.
            if (!message.Contains("&") || !message.Contains("\t"))
                return message;

            // Split the message by \t, into an array of bits. (chunks)
            String[] bits = message.Split('\t');

            // Allocate strings for reuse
            String parsed = String.Empty;
            String bit    = String.Empty;
            String lump   = String.Empty;

            // Assign some basic values for reuse
            int index = 0, last_index = 0, amp_pos = 0;

            // Loop through each bit of the string
            for (int p = 0; p < bits.Length; p++)
            {
                // Grab the string at the current position
                bit = bits[p];

                // We don't do anything if there's no & in the string.
                if (bit.Contains("&"))
                {
                    // Everything before the lump gets thrown into the parsed string as-is
                    amp_pos = bit.IndexOf('&');
                    parsed += bit.Substring(0, amp_pos);

                    // If there is one or more & in the string, let's loop through their indices
                    while ((index = bit.IndexOf('&', last_index)) != -1)
                    {
                        lump = bit.Substring(index + 1);
                        last_index = index + 1;

                        // If there's another lump inside out substring, only go up to that position.
                        if (lump.Contains("&"))
                        {
                            lump = lump.Substring(0, lump.IndexOf('&'));
                        }

                        // We don't know this lump. No reason to parse it, so just add it as-is.
                        if (!lump_arg_count.ContainsKey(lump))
                        {
                            // Make sure to give the & back to non-tablumps!
                            parsed += "&" + lump;
                            continue;
                        }

                        // Get the argument count for this lump
                        int arg_count = lump_arg_count[lump];

                        // If the arg count is zero, we only need to surround it with <>
                        if (arg_count == 0)
                        {
                            parsed += "<" + lump + ">";
                        }
                        else
                        {
                            // Otherwise, we need to act according to the tag.
                            if (lump == "abbr" || lump == "acro")
                            {
                                // One argument.
                                // We need to parse the tablumps of the title, since it'd go untouched.
                                parsed += "<" + lump + " title=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "a")
                            {
                                // One argument.
                                parsed += "<a href=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "avatar")
                            {
                                // Two arguments. We use the first
                                parsed += ":icon" + bits[p + 1] + ":";
                            }
                            else if (lump == "dev")
                            {
                                // Two argument. We use the second
                                parsed += ":dev" + bits[p + 2] + ":";
                            }
                            else if (lump == "img")
                            {
                                // Three arguments. We're only going to use the first (src)
                                //  and ignore the other two (height and width).
                                parsed += "<img src=\"" + bits[p + 1] + "\" />";
                            }
                            else if (lump == "iframe" || lump == "embed")
                            {
                                // Three arguments. We're only going to use the first (src)
                                //  and ignore the other two (height and width).
                                parsed += "<" + lump + " src=\"" + bits[p + 1] + "\">";
                            }
                            else if (lump == "emote")
                            {
                                // Five arguments. We're only going to use the first (emote code)
                                parsed += bits[p + 1];
                            }
                            else if (lump == "thumb")
                            {
                                // Seven arguments. We're only going to use the first (thumb code)
                                parsed += ":thumb" + bits[p + 1] + ":";
                            }
                            else if (lump == "link")
                            {
                                // Two OR three arguments. Odd one.
                                // We just use the first, in a way, instead of getting the title.
                                parsed += "<a href=\"" + bits[p + 1] + "\">[link]</a>";
                                if (bits[p + 2] != "&")
                                    arg_count++;
                            }
                        }

                        p += arg_count;

                        // If we pass the end of the string, we're done with this bit.
                        if (last_index >= bit.Length)
                            break;
                    }

                    // Clear index and last_index
                    index = 0;
                    last_index = 0;
                }
                else
                {
                    // It's not a tablump, so throw it into the parsed string as-is
                    parsed += bit;
                }
            }

            // Return the parsed message
            return parsed;
        }

        /// <summary>
        /// Generates an MD5 hash for the specified input
        /// </summary>
        /// <param name="input">Plaintext string</param>
        /// <returns>MD5 hash in string format</returns>
        public static String md5(String input)
        {
            StringBuilder output = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                for (int i = 0; i < data.Length; i++)
                {
                    output.Append(data[i].ToString("x2"));
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats a specified amount of seconds into a human readable string.
        /// i.e. 3669 becomes:
        /// 1 hour, 1 minute, 9 seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds</param>
        /// <returns>Human readable string</returns>
        public static String FormatTime(int seconds)
        {
            String output = String.Empty;

            if (seconds == 0)
                return "0 seconds.";

            int days = 0, hours = 0, minutes = 0;

            while (seconds >= 86400)
            {
                ++days;
                seconds -= 86400;
            }

            while (seconds >= 3600)
            {
                ++hours;
                seconds -= 3600;
            }

            while (seconds >= 60)
            {
                ++minutes;
                seconds -= 60;
            }

            if (days > 0)
                output += days + " day" + (days == 1 ? "" : "s") + ", ";

            if (hours > 0)
                output += hours + " hour" + (hours == 1 ? "" : "s") + ", ";

            if (minutes > 0)
                output += minutes + " minute" + (minutes == 1 ? "" : "s") + ", ";

            if (seconds > 0)
                output += seconds + " second" + (seconds == 1 ? "" : "s") + ".";
            else if (output.Length > 0)
                output = output.Substring(0, output.Length - 2) + ".";

            return output;
        }

        /// <summary>
        /// Writes or appends text to a file
        /// </summary>
        /// <param name="filename">filename with path</param>
        /// <param name="content">content to write</param>
        /// <param name="append">append to the end or overwrite the file</param>
        public static void WriteFile(String filename, String content, bool append = false)
        {
            if (!filename.Contains("/") || filename.Contains("..") || filename.Contains("~"))
            {
                ConIO.Warning("Tools.WriteFile", "Cannot write to (or below) bot's root directory.");
                return;
            }

            String dir = Path.GetDirectoryName(filename);

            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(filename) || !append)
                    File.Create(filename).Close();

                using (StreamWriter writer = File.AppendText(filename))
                {
                    writer.Write(content);
                }
            }
            catch (Exception E)
            {
                ConIO.Warning("Tools.WriteFile", "Caught Exception: " + E.ToString());
            }
        }
    }
}
