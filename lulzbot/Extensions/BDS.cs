using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;
using System.Timers;

namespace lulzbot.Extensions
{
    public class BDS
    {
        private static Dictionary<String, Types.BotDef> _botdef_database            = new Dictionary<string, Types.BotDef>();
        private static Dictionary<String, Types.BotInfo> _botinfo_database          = new Dictionary<string, Types.BotInfo>();
        private static Dictionary<String, Types.ClientInfo> _clientinfo_database    = new Dictionary<string, Types.ClientInfo>();
        private static Dictionary<String, String> _info_requests                    = new Dictionary<string, string>();

        private const int UPDATE_TIME = 604800;

        /// <summary>
        /// Set this to false to overwrite automated saving of the database.
        /// </summary>
        public static bool AutoSave = true;

        public BDS()
        {
            Events.AddEvent("recv_msg",     new Event(this, "ParseBDS", "Parses BDS messages."));
            Events.AddEvent("join",         new Event(this, "evt_onjoin", "Handles BDS related actions on joining datashare."));
            
            Events.AddCommand("bot",        new Command(this, "cmd_bot", "DivinityArcane", 25, "Gets information from the database."));
            Events.AddCommand("bds",        new Command(this, "cmd_bds", "DivinityArcane", 75, "Manage BDS database."));

            if (Program.Debug)
                ConIO.Write("Loading databases...", "BDS");

            // Load saved data, if we can.
            _botdef_database        = Storage.Load<Dictionary<String, Types.BotDef>>("bds_botdef_database");
            _botinfo_database       = Storage.Load<Dictionary<String, Types.BotInfo>>("bds_botinfo_database");
            _clientinfo_database    = Storage.Load<Dictionary<String, Types.ClientInfo>>("bds_clientinfo_database");

            // Values can be null if the file is empty or doesn't exist.
            if (_botdef_database == null)
                _botdef_database = new Dictionary<string, Types.BotDef>();

            if (_botinfo_database == null)
                _botinfo_database = new Dictionary<string, Types.BotInfo>();

            if (_clientinfo_database == null)
                _clientinfo_database = new Dictionary<string, Types.ClientInfo>();

            if (Program.Debug)
                ConIO.Write(String.Format("Loaded databases. Got {0} BotDEF entries, {1} BotINFO entries, and {2} ClientINFO entries.", _botdef_database.Count, _botinfo_database.Count, _clientinfo_database.Count), "BDS");

            // We will save on a timer. 
            if (AutoSave)
            {
                // Saves once per five minutes.
                Timer save_timer = new Timer(300000);

                save_timer.Elapsed += delegate { if (BDS.AutoSave) BDS.Save(); };

                save_timer.Start();
            }
        }

        public static void evt_onjoin(Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter.ToLower() == "chat:datashare")
            {
                // IDS-NOTE, XFER, BOTCHECK-SYNC ?
                String[] caps = new String[] { "BOTCHECK", "BOTCHECK-EXT", "LDS-UPDATE", "LDS-BOTCHECK" };
                bot.Say(packet.Parameter, "BDS:PROVIDER:CAPS:" + String.Join(",", caps));
            }
        }

        /// <summary>
        /// Saves the databases to disk
        /// </summary>
        private static void Save()
        {
            if (Program.Debug)
                ConIO.Write("Saving databases.", "BDS");

            Storage.Save("bds_botdef_database", _botdef_database);
            Storage.Save("bds_botinfo_database", _botinfo_database);
            Storage.Save("bds_clientinfo_database", _clientinfo_database);
        }

        /// <summary>
        /// Checks whether the specified username is a policebot
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <param name="channel">Channel to check. Default: #DataShare</param>
        /// <returns>true if PoliceBot, false otherwise.</returns>
        private static bool IsPoliceBot(String username, String channel = "chat:datashare")
        {
            channel = channel.ToLower();

            if (!Core.ChannelData.ContainsKey(channel))
                return false;

            if (!Core.ChannelData[channel].Members.ContainsKey(username.ToLower()))
                return false;

            if (Core.ChannelData[channel].Members[username.ToLower()].Privclass.ToLower() == "policebot")
                return true;
            else
                return false;
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_bot(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>&raquo; {0}bot info [username]<br/>&raquo; {0}bot count", bot.Config.Trigger);
            
            // First arg is the command
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                if (args[1] == "info")
                {
                    if (args.Length >= 3)
                    {
                        if (_botinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            Types.BotInfo info = _botinfo_database[args[2].ToLower()];
                            int ts = Bot.EpochTimestamp - info.Modified;
                            if (ts >= UPDATE_TIME) // 7 days
                            {
                                lock (_info_requests)
                                {
                                    _info_requests.Add(args[2].ToLower(), ns);
                                }

                                bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                                bot.Say(ns, String.Format("{0}: Data for {1} is outdated, one second while I update it...", from, args[2]));
                                return;
                            }
                            String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", info.Name);
                            output += String.Format("<b>Bot type:</b> {0}<br/>", info.Type);
                            output += String.Format("<b>Bot version:</b> {0}<br/>", info.Version);
                            output += String.Format("<b>Bot owner:</b> :dev{0}:<br/>", info.Owner);
                            output += String.Format("<b>Bot trigger:</b> <b><code>{0}</code></b><br/>", info.Trigger.Replace("&", "&amp;"));
                            output += String.Format("<b>BDS version:</b> {0}<br/>", info.BDSVersion);
                            output += String.Format("<b>Last modified:</b> {0} ago", Tools.FormatTime(ts).TrimEnd('.'));
                            bot.Say(ns, output);
                        }
                        else
                        {
                            lock (_info_requests)
                            {
                                _info_requests.Add(args[2].ToLower(), ns);
                            }

                            bot.NPSay("chat:datashare", "BDS:BOTCHECK:REQUEST:" + args[2]);
                            bot.Say(ns, String.Format("{0}: {1} isn't in my database yet. Requesting information, please stand by...", from, args[2]));
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else if (args[1] == "count")
                {
                    bot.Say(ns, String.Format("<b>&raquo;</b> There are {0} bot{1} in my local database.", _botinfo_database.Count, _botinfo_database.Count == 1 ? "" : "s"));
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_bds(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            // First arg is the command
            if (args.Length == 1)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b><br/>{0}bds save<br/>{0}bds update", " &middot; " + bot.Config.Trigger));
            }
            else
            {
                String arg = args[1].ToLower();

                if (arg == "save")
                {
                    Save();
                    bot.Say(ns, "<b>&raquo; Database has been saved to disk.</b>");
                }
                else if (arg == "update")
                {
                    List<String> bots = new List<String>();

                    if (Core.ChannelData.ContainsKey("chat:datashare"))
                    {
                        ChatData cd = Core.ChannelData["chat:datashare"];

                        foreach (ChatMember m in cd.Members.Values)
                        {
                            if (m.Privclass == "Bots")
                            {
                                if (!_botinfo_database.ContainsKey(m.Name.ToLower()) || Bot.EpochTimestamp - _botinfo_database[m.Name.ToLower()].Modified >= UPDATE_TIME)
                                {
                                    bots.Add(m.Name);
                                }
                            }
                        }
                    }

                    if (bots.Count > 0)
                    {
                        bot.Say("chat:DataShare", "BDS:BOTCHECK:REQUEST:" + String.Join(",", bots));
                        bot.Say(ns, String.Format("<b>&raquo; Requested data for {0} bot{1}.</b>", bots.Count, bots.Count == 1 ? "" : "s"));
                    }
                    else
                        bot.Say(ns, "<b>&raquo; No data needs to be updated.</b>");
                }
            }
        }

        /// <summary>
        /// Parses BDS messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="packet">Packet object</param>
        public void ParseBDS(Bot bot, dAmnPacket packet)
        {
            // Not from DS? Ignore it.
            if (packet.Parameter.ToLower() != "chat:datashare")
                return;

            // Doesn't contain segments? Ignore it.
            if (!packet.Body.Contains(":"))
                return;

            String msg      = packet.Body;
            String[] bits   = msg.Split(':');
            String ns       = packet.Parameter;
            String from     = packet.Arguments["from"];
            String username = bot.Config.Username;
            String trigger  = bot.Config.Trigger;
            String owner    = bot.Config.Owner;

            bool from_policebot = IsPoliceBot(from, packet.Parameter);

            if (bits[0] == "BDS")
            {
                if (bits.Length >= 3 && bits[1] == "BOTCHECK")
                {
                    if (bits[2] == "ALL" || (bits.Length >= 4 && bits[2] == "DIRECT" && bits[3].ToLower() == username.ToLower()))
                    {
                        // If it's not a police bot, return.
                        if (!from_policebot)
                            return;

                        String hashkey = Tools.md5((trigger + from + username).ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/0.3,{4},{5}", from, owner, Program.BotName, Program.Version, hashkey, trigger));
                    }
                    else if (bits[2] == "DIRECT" && bits[3].ToLower().Contains(","))
                    {
                        List<String> bots = new List<String>(bits[3].ToLower().Split(new char[] { ',' }));

                        if (bots.Contains(username.ToLower()))
                        {
                            // If it's not a police bot, return.
                            if (!from_policebot)
                                return;

                            String hashkey = Tools.md5((trigger + from + username).ToLower());
                            bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/0.3,{4},{5}", from, owner, Program.BotName, Program.Version, hashkey, trigger));
                        }
                    }
                    else if (bits[2] == "RESPONSE" && bits.Length >= 4)
                    {
                        // Look for a valid string
                        if (!bits[3].Contains(","))
                            return;

                        // Possibly add privclass/client checks

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 6 || !data[3].Contains("/"))
                            return;

                        String[] versions = data[3].Split('/');
                        String botver = versions[0];
                        String hash = data[4];
                        String trig = data[5];
                        double bdsver = 0.0;

                        // trigger contains a comma?
                        if (data.Length > 6)
                        {
                            trig = String.Empty;
                            for (int b = 6; b < data.Length; b++)
                            {
                                if (b >= data.Length - 1)
                                    trig += data[b];
                                else
                                    trig += data[b] + ",";
                            }
                        }

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.BotInfo bot_info = new Types.BotInfo(from, data[1], data[2], botver, trig, bdsver, Bot.EpochTimestamp);

                        String hashkey = Tools.md5((trig + data[0] + from).ToLower()).ToLower();

                        if (hashkey != hash)
                        {
                            // Invalid hash supplied
                            // For now, we ignore this. Though I'd like to see policebots send and error like:
                            //  BDS:BOTCHECK:ERROR:INVALID_RESPONSE_HASH
                        }
                        else
                        {
                            lock (_botinfo_database)
                            {
                                if (_botinfo_database.ContainsKey(from.ToLower()))
                                {
                                    _botinfo_database[from.ToLower()] = bot_info;

                                    if (Program.Debug)
                                        ConIO.Write("Updated database for bot: " + from, "BDS");
                                }
                                else
                                {
                                    _botinfo_database.Add(from.ToLower(), bot_info);

                                    if (Program.Debug)
                                        ConIO.Write("Added bot to database: " + from, "BDS");
                                }
                            }
                        }

                    }
                    else if (bits.Length >= 4 && bits[2] == "INFO")
                    {
                        if (!bits[3].Contains(","))
                            return;

                        // Handle it
                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String[] data = input.Split(',');

                        // Invalid data
                        if (data.Length < 5 || !data[2].Contains("/"))
                            return;

                        String[] versions = data[2].Split('/');
                        String botver = versions[0];
                        String trig = data[4];
                        double bdsver = 0.0;

                        // trigger contains a comma?
                        if (data.Length > 5)
                        {
                            trig = String.Empty;
                            for (int b = 5; b < data.Length; b++)
                            {
                                if (b >= data.Length - 1)
                                    trig += data[b];
                                else
                                    trig += data[b] + ",";
                            }
                        }

                        if (!Double.TryParse(versions[1], out bdsver))
                            bdsver = 0.2;

                        Types.BotInfo bot_info = new Types.BotInfo(data[0], data[3], data[1], botver, trig, bdsver, Bot.EpochTimestamp);

                        lock (_botinfo_database)
                        {
                            if (_botinfo_database.ContainsKey(data[0].ToLower()))
                            {
                                _botinfo_database[data[0].ToLower()] = bot_info;

                                if (Program.Debug)
                                    ConIO.Write("Updated database for bot: " + data[0], "BDS");
                            }
                            else
                            {
                                _botinfo_database.Add(data[0].ToLower(), bot_info);

                                if (Program.Debug)
                                    ConIO.Write("Added bot to database: " + data[0], "BDS");
                            }
                        }

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());

                                String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", bot_info.Name);
                                output += String.Format("<b>Bot type:</b> {0}<br/>", bot_info.Type);
                                output += String.Format("<b>Bot version:</b> {0}<br/>", bot_info.Version);
                                output += String.Format("<b>Bot owner:</b> :dev{0}:<br/>", bot_info.Owner);
                                output += String.Format("<b>Bot trigger:</b> <b><code>{0}</code></b><br/>", bot_info.Trigger.Replace("&", "&amp;"));
                                output += String.Format("<b>BDS version:</b> {0}<br/>", bot_info.BDSVersion);
                                bot.Say(chan, output);
                            }
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "NODATA")
                    {
                        // Ignore data from non-police bots
                        if (!from_policebot)
                            return;

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(bits[3].ToLower()))
                            {
                                String chan = _info_requests[bits[3].ToLower()];
                                _info_requests.Remove(bits[3].ToLower());
                                bot.Say(chan, "<b>&raquo; Bot doesn't exist:</b> " + bits[3]);
                            }
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "BADBOT")
                    {
                        // Ignore data from non-police bots
                        if (!from_policebot)
                            return;

                        if (!bits[3].Contains(","))
                            return;

                        String[] data = bits[3].Split(',');

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());
                                bot.Say(chan, "<b>&raquo; Bot is banned:</b> " + data[0]);
                            }
                        }

                        // Maybe store this later.
                    }
                }
                else if (bits.Length >= 4 && bits[1] == "BOTDEF")
                {
                    if (bits[2] == "REQUEST" && bits[3] == username.ToLower())
                    {
                        // If it's not the police bot, return.
                        if (from_policebot)
                            return;

                        String hashkey = Tools.md5((from + Program.BotName + "DivinityArcane").ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTDEF:RESPONSE:{0},{1},{2},{3},{4},{5}", from, Program.BotName, "C#", "DivinityArcane", "http://botdom.com/wiki/User:Kyogo/lulzBot", hashkey));
                    }
                }
            }
            else if (bits[0] == "LDS")
            {
                if (bits.Length >= 4 && bits[1] == "UPDATE")
                {
                    if (bits[2] == "PING" && bits[3].ToLower() == username.ToLower())
                    {
                        bot.NPSay(ns, String.Format("LDS:UPDATE:PONG:{0},{1},{2}", from, Program.BotName, Program.Version));
                    }
                    else if (bits[2] == "NOTIFY")
                    {
                        if (from_policebot || from.ToLower() == "divinityarcane")
                        {
                            if (bits[3].Contains(","))
                            {
                                String[] pars = bits[3].Split(new char[] {','});
                                if (pars.Length == 3 && pars[0].ToLower() == username.ToLower())
                                {
                                    int secs = 0;
                                    bool ok = int.TryParse(pars[2], out secs);
                                    if (ok)
                                    {
                                        ConIO.Notice(String.Format("A new version of lulzBot is available: version {0} (Released {1} ago)", pars[1], Tools.FormatTime(Tools.Timestamp() - secs)));
                                        //ConIO.Notice(String.Format("To update, use the update command."));
                                    }
                                }
                            }
                        }
                    }
                }
                else if (bits.Length >= 3 && bits[1] == "BOTCHECK")
                {
                    if (bits[2] == "ALL")
                    {
                        if (from_policebot || from.ToLower() == "divinityarcane")
                        {
                            // from, owner, botname, botversion, uptime, disconnects, bytes_sent, bytes_received
                            bot.NPSay(ns, String.Format("LDS:BOTCHECK:RESPONSE:{0},{1},{2},{3},{4},{5},{6},{7}",
                                from, owner, Program.BotName, Program.Version, bot.uptime, Program.Disconnects, Program.bytes_sent, Program.bytes_received));
                        }
                    }
                    else if (bits.Length >= 4 && bits[2] == "DIRECT" && bits[3].ToLower() == username.ToLower())
                    {
                        // from, owner, botname, botversion, uptime, disconnects, bytes_sent, bytes_received
                        bot.NPSay(ns, String.Format("LDS:BOTCHECK:RESPONSE:{0},{1},{2},{3},{4},{5},{6},{7}",
                            from, owner, Program.BotName, Program.Version, bot.uptime, Program.Disconnects, Program.bytes_sent, Program.bytes_received));
                    }
                }
            }
        }
    }
}
