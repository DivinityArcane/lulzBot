﻿using lulzbot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace lulzbot.Extensions
{
    public class BDS
    {
        public static Dictionary<String, Types.BotDef> _botdef_database             = new Dictionary<String, Types.BotDef>();
        public static Dictionary<String, Types.BotInfo> _botinfo_database           = new Dictionary<String, Types.BotInfo>();
        public static Dictionary<String, Types.ClientInfo> _clientinfo_database     = new Dictionary<String, Types.ClientInfo>();
        private static Dictionary<String, String> _info_requests                    = new Dictionary<String, String>();
        public static List<String> TranslateLangs                                   = new List<String>() { "ar", "bg", "zh-CN", "hr", "cs", "da", "nl", "en", "fi", "fr", "de", "el", "hi", "it", "ja", "ko", "no", "pl", "pt", "ro", "ru", "es", "sv" };
        public static Dictionary<String, String> LanguageAliases                    = new Dictionary<String, String>() { { "arabic", "ar" }, { "bulgarian", "bg" }, { "chinese", "zh-CN" }, { "croatian", "hr" }, { "czech", "cs" }, { "danish", "da" }, { "dutch", "nl" }, { "english", "en" }, { "finnish", "fi" }, { "french", "fr" }, { "german", "de" }, { "greek", "el" }, { "hindi", "hi" }, { "italian", "it" }, { "japanese", "ja" }, { "korean", "ko" }, { "norwegian", "no" }, { "polish", "pl" }, { "portugese", "pt" }, { "romanian", "ro" }, { "russian", "ru" }, { "spanish", "es" }, { "swedish", "sv" } };
        private static List<String> _translate_requests                             = new List<String>();
        private static List<String> _botcheck_privclasses                           = new List<String>() { "Bots", "TestBots", "BrokenBots", "SuspiciousBots", "PoliceBot" };
        private static List<String> _clientcheck_privclasses                        = new List<String>() { "Clients", "BrokenClients", "Members", "Seniors", "CoreTeam" };
        private const int UPDATE_TIME = 604800;
        public const double Version = 0.4;

        /// <summary>
        /// Set this to false to overwrite automated saving of the database.
        /// </summary>
        public static bool AutoSave = true;

        public BDS ()
        {
            Events.AddEvent("recv_msg", new Event(this, "ParseBDS", "Parses BDS messages."));
            Events.AddEvent("join", new Event(this, "evt_onjoin", "Handles BDS related actions on joining datashare."));

            Events.AddCommand("bot", new Command(this, "cmd_bot", "DivinityArcane", 25, "Gets information from the database."));
            Events.AddCommand("client", new Command(this, "cmd_client", "DivinityArcane", 25, "Gets information from the database."));
            Events.AddCommand("bds", new Command(this, "cmd_bds", "DivinityArcane", 75, "Manage BDS database."));
            Events.AddCommand("translate", new Command(this, "cmd_translate", "DivinityArcane", 25, "Translates text using BDS."));

            if (Program.Debug)
                ConIO.Write("Loading databases...", "BDS");

            // Load saved data, if we can.
            _botdef_database = Storage.Load<Dictionary<String, Types.BotDef>>("bds_botdef_database");
            _botinfo_database = Storage.Load<Dictionary<String, Types.BotInfo>>("bds_botinfo_database");
            _clientinfo_database = Storage.Load<Dictionary<String, Types.ClientInfo>>("bds_clientinfo_database");

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

        public static void evt_onjoin (Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter.ToLower() == "chat:datashare")
            {
                // IDS-NOTE, XFER, BOTCHECK-SYNC ?
                String[] caps = new String[] { "BOTCHECK", "BOTCHECK-EXT", "LDS-UPDATE", "LDS-BOTCHECK" };
                bot.NPSay(packet.Parameter, "BDS:PROVIDER:CAPS:" + String.Join(",", caps));
            }
        }

        /// <summary>
        /// Saves the databases to disk
        /// </summary>
        private static void Save ()
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
        private static bool IsPoliceBot (String username, String channel = "chat:datashare")
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
        public void cmd_bot (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>&raquo; {0}bot info [username]<br/>&raquo; {0}bot count<br/>&raquo; {0}bot online [type]", bot.Config.Trigger);

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
                        else if (_clientinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} is a client. Use {1}client info {0}</b>", args[2], bot.Config.Trigger));
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
                    if (_botinfo_database.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; There are 0 bots in my local database.</b>");
                        return;
                    }

                    Dictionary<String, int> bots = new Dictionary<string, int>();

                    foreach (BotInfo info in _botinfo_database.Values)
                    {
                        if (!bots.ContainsKey(info.Type))
                            bots.Add(info.Type, 0);

                        bots[info.Type]++;
                    }

                    var bots_sorted =    from pair in bots
                                         orderby pair.Value descending
                                         select pair;

                    String output = String.Empty;
                    int count = 0;

                    foreach (KeyValuePair<String, int> pair in bots_sorted)
                    {
                        output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                        count += pair.Value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; There are {0} bot{1} in my local database:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                }
                else if (args[1] == "online")
                {
                    String type = "all";
                    if (args.Length >= 3)
                    {
                        type = msg.Substring(11).ToLower();
                    }

                    if (type == "all")
                    {
                        Dictionary<String, int> bots = new Dictionary<string, int>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_botcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_botinfo_database.ContainsKey(m.Name.ToLower()))
                                    {
                                        if (!bots.ContainsKey(_botinfo_database[m.Name.ToLower()].Type))
                                            bots.Add(_botinfo_database[m.Name.ToLower()].Type, 0);

                                        bots[_botinfo_database[m.Name.ToLower()].Type]++;
                                    }
                                }
                            }
                        }

                        if (bots.Count == 0)
                        {
                            bot.Say(ns, "<b>&raquo; 0 known online bots.</b>");
                            return;
                        }

                        var bots_sorted =    from pair in bots
                                             orderby pair.Value descending
                                             select pair;

                        String output = String.Empty;
                        int count = 0;

                        foreach (KeyValuePair<String, int> pair in bots_sorted)
                        {
                            output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                            count += pair.Value;
                        }

                        bot.Say(ns, String.Format("<b>&raquo; {0} known online bot{1}:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                    }
                    else
                    {
                        List<String> bots = new List<string>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_botcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_botinfo_database.ContainsKey(m.Name.ToLower()) && _botinfo_database[m.Name.ToLower()].Type.ToLower() == type)
                                        bots.Add(m.Name);
                                }
                            }
                        }

                        bots.Sort();

                        if (bots.Count > 0)
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} known online bot{1} of type {2}:</b><br/><b>&raquo; [</b>{3}<b>]</b>", bots.Count, bots.Count == 1 ? "" : "s", type, String.Join("<b>]</b>, <b>[</b>", bots)));
                        }
                        else
                            bot.Say(ns, String.Format("<b>&raquo; 0 known online bots of type {0}.</b>", type));
                    }
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_client (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>&raquo; {0}client info [username]<br/>&raquo; {0}client count<br/>&raquo; {0}client online [type]", bot.Config.Trigger);

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
                        if (_clientinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            Types.ClientInfo info = _clientinfo_database[args[2].ToLower()];
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
                            output += String.Format("<b>Client type:</b> {0}<br/>", info.Type);
                            output += String.Format("<b>Client version:</b> {0}<br/>", info.Version);
                            output += String.Format("<b>BDS version:</b> {0}<br/>", info.BDSVersion);
                            output += String.Format("<b>Last modified:</b> {0} ago", Tools.FormatTime(ts).TrimEnd('.'));
                            bot.Say(ns, output);
                        }
                        else if (_botinfo_database.ContainsKey(args[2].ToLower()))
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} is a bot. Use {1}bot info {0}</b>", args[2], bot.Config.Trigger));
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
                    if (_clientinfo_database.Count == 0)
                    {
                        bot.Say(ns, "<b>&raquo; There are 0 clients in my local database.</b>");
                        return;
                    }

                    Dictionary<String, int> clients = new Dictionary<string, int>();

                    foreach (ClientInfo info in _clientinfo_database.Values)
                    {
                        if (!clients.ContainsKey(info.Type))
                            clients.Add(info.Type, 0);

                        clients[info.Type]++;
                    }

                    var clients_sorted = from pair in clients
                                         orderby pair.Value descending
                                         select pair;

                    String output = String.Empty;
                    int count = 0;

                    foreach (KeyValuePair<String, int> pair in clients_sorted)
                    {
                        output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                        count += pair.Value;
                    }

                    bot.Say(ns, String.Format("<b>&raquo; There are {0} client{1} in my local database:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                }
                else if (args[1] == "online")
                {
                    String type = "all";
                    if (args.Length >= 3)
                    {
                        type = msg.Substring(14).ToLower();
                    }

                    if (type == "all")
                    {
                        Dictionary<String, int> clients = new Dictionary<string, int>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_clientcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_clientinfo_database.ContainsKey(m.Name.ToLower()))
                                    {
                                        if (!clients.ContainsKey(_clientinfo_database[m.Name.ToLower()].Type))
                                            clients.Add(_clientinfo_database[m.Name.ToLower()].Type, 0);

                                        clients[_clientinfo_database[m.Name.ToLower()].Type]++;
                                    }
                                }
                            }
                        }

                        if (clients.Count == 0)
                        {
                            bot.Say(ns, "<b>&raquo; 0 known online clients.</b>");
                            return;
                        }

                        var clients_sorted = from pair in clients
                                             orderby pair.Value descending
                                             select pair;

                        String output = String.Empty;
                        int count = 0;

                        foreach (KeyValuePair<String, int> pair in clients_sorted)
                        {
                            output += String.Format("{0} ({1})<b>]</b>, <b>[</b>", pair.Key, pair.Value);
                            count += pair.Value;
                        }

                        bot.Say(ns, String.Format("<b>&raquo; {0} known online client{1}:</b><br/><b>&raquo; [</b>", count, count == 1 ? "" : "s") + output.Substring(0, output.Length - 10));
                    }
                    else
                    {
                        List<String> clients = new List<string>();

                        if (Core.ChannelData.ContainsKey("chat:datashare"))
                        {
                            ChatData cd = Core.ChannelData["chat:datashare"];

                            foreach (ChatMember m in cd.Members.Values)
                            {
                                if (_clientcheck_privclasses.Contains(m.Privclass))
                                {
                                    if (_clientinfo_database.ContainsKey(m.Name.ToLower()) && _clientinfo_database[m.Name.ToLower()].Type.ToLower() == type)
                                        clients.Add(m.Name);
                                }
                            }
                        }

                        clients.Sort();

                        if (clients.Count > 0)
                        {
                            bot.Say(ns, String.Format("<b>&raquo; {0} known online client{1} of type {2}:</b><br/><b>&raquo; [</b>{3}<b>]</b>", clients.Count, clients.Count == 1 ? "" : "s", type, String.Join("<b>]</b>, <b>[</b>", clients)));
                        }
                        else
                            bot.Say(ns, String.Format("<b>&raquo; 0 known online clients of type {0}.</b>", type));
                    }
                }
            }
        }

        /// <summary>
        /// BDS command
        /// </summary>
        public void cmd_bds (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
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
                    List<String> datas = new List<String>();

                    if (Core.ChannelData.ContainsKey("chat:datashare"))
                    {
                        ChatData cd = Core.ChannelData["chat:datashare"];

                        foreach (ChatMember m in cd.Members.Values)
                        {
                            if (_botcheck_privclasses.Contains(m.Privclass))
                            {
                                if (!_botinfo_database.ContainsKey(m.Name.ToLower()) || Bot.EpochTimestamp - _botinfo_database[m.Name.ToLower()].Modified >= UPDATE_TIME)
                                {
                                    datas.Add(m.Name);
                                }
                            }
                            else if (_clientcheck_privclasses.Contains(m.Privclass))
                            {
                                if (!_clientinfo_database.ContainsKey(m.Name.ToLower()) || Bot.EpochTimestamp - _clientinfo_database[m.Name.ToLower()].Modified >= UPDATE_TIME)
                                {
                                    datas.Add(m.Name);
                                }
                            }
                        }
                    }

                    if (datas.Count > 0)
                    {
                        bot.NPSay("chat:DataShare", "BDS:BOTCHECK:REQUEST:" + String.Join(",", datas));
                        bot.Say(ns, String.Format("<b>&raquo; Requested data for {0} bot{1}/client{1}.</b>", datas.Count, datas.Count == 1 ? "" : "s"));
                    }
                    else
                        bot.Say(ns, "<b>&raquo; No data needs to be updated.</b>");
                }
            }
        }

        public void cmd_translate (Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}translate languages<br/>{0}translate <i>from_lang to_lang</i> message", " &middot; " + bot.Config.Trigger);
            if (args.Length == 1)
            {
                bot.Say(ns, helpmsg);
            }
            else
            {
                if (args[1] == "languages")
                {
                    String output = String.Format("<b>&raquo; There are {0} supported language{1}:</b><br/><br/>", TranslateLangs.Count, TranslateLangs.Count == 1 ? "" : "s");

                    foreach (var pair in LanguageAliases)
                    {
                        output += String.Format("<b>[{0}:</b> {1}<b>]</b> &nbsp; ", pair.Key, pair.Value);
                    }

                    output += "<br/><br/><sub><i>* Note that at least one of the languages used in translation must be English.</i></sub>";

                    bot.Say(ns, output);
                }
                else
                {
                    if (args.Length > 3)
                    {
                        String from_lang = args[1].ToLower(), to_lang = args[2].ToLower();

                        if (!TranslateLangs.Contains(from_lang))
                        {
                            if (LanguageAliases.ContainsKey(from_lang))
                                from_lang = LanguageAliases[from_lang];
                            else
                            {
                                bot.Say(ns, "<b>&raquo; Invalid from_lang.</b>");
                                return;
                            }
                        }

                        if (!TranslateLangs.Contains(to_lang))
                        {
                            if (LanguageAliases.ContainsKey(to_lang))
                                to_lang = LanguageAliases[to_lang];
                            else
                            {
                                bot.Say(ns, "<b>&raquo; Invalid to_lang.</b>");
                                return;
                            }
                        }

                        if (from_lang != "en" && to_lang != "en")
                        {
                            bot.Say(ns, "<b>&raquo; At least one of the languages must be English!</b>");
                            return;
                        }

                        String message = Convert.ToBase64String(Encoding.UTF8.GetBytes(WebUtility.HtmlDecode(msg.Substring(11 + args[1].Length + args[2].Length))));

                        lock (_translate_requests)
                        {
                            _translate_requests.Add(packet.Parameter);
                            bot.NPSay("chat:datashare", String.Format("BDS:TRANSLATE:REQUEST:{0},{1},{2},{3}", packet.Parameter, from_lang, to_lang, message));
                        }
                    }
                    else bot.Say(ns, helpmsg);
                }
            }
        }

        /// <summary>
        /// Parses BDS messages
        /// </summary>
        /// <param name="bot">Bot instance</param>
        /// <param name="packet">Packet object</param>
        public void ParseBDS (Bot bot, dAmnPacket packet)
        {
            if (packet.Parameter == "chat:Botdom" && packet.Body.ToLower().StartsWith("<abbr title=\"" + bot.Config.Username.ToLower() + ": botcheck\"></abbr>"))
            {
                String hash = Tools.md5((bot.Config.Trigger + packet.Arguments["from"] + bot.Config.Username).ToLower()).Replace(" ", "").ToLower();
                bot.Say(packet.Parameter, String.Format("Beep! <abbr title=\"botresponse: {0} {1} {2} {3} {4} {5}\"></abbr>", packet.Arguments["from"], bot.Config.Owner, Program.BotName, Program.Version, hash, bot.Config.Trigger));
                return;
            }

            // Not from DS? Ignore it.
            if (packet.Parameter.ToLower() != "chat:datashare" && packet.Parameter.ToLower() != "chat:dsgateway")
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
                    if (bits[2] == "OK" && bits.Length >= 4 && bits[3].ToLower() == username.ToLower())
                    {
                        if (!from_policebot)
                            return;

                        bot.Join("chat:DataShare");
                        bot.Part("chat:DSGateWay");
                    }
                    else if (bits[2] == "DENIED" && bits.Length >= 4 && bits[3].ToLower().StartsWith(username.ToLower() + ','))
                    {
                        if (!from_policebot)
                            return;

                        // Look for a valid string
                        if (!bits[3].Contains(","))
                            return;

                        String input = String.Empty;

                        for (byte b = 3; b < bits.Length; b++)
                        {
                            if (b >= bits.Length - 1)
                                input += bits[b];
                            else
                                input += bits[b] + ":";
                        }

                        String reason = input.Substring(username.Length + 1);

                        ConIO.Warning("#DataShare", "Denied access: " + reason);

                        bot.Part("chat:DSGateway");
                    }
                    else if (bits[2] == "ALL" || (bits.Length >= 4 && bits[2] == "DIRECT" && bits[3].ToLower() == username.ToLower()))
                    {
                        // If it's not a police bot, return.
                        if (!from_policebot)
                            return;

                        String hashkey = Tools.md5((trigger + from + username).Replace(" ", "").ToLower());
                        bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/{4},{5},{6}", from, owner, Program.BotName, Program.Version, Version, hashkey, trigger));
                    }
                    else if (bits[2] == "DIRECT" && bits[3].ToLower().Contains(","))
                    {
                        List<String> bots = new List<String>(bits[3].ToLower().Split(new char[] { ',' }));

                        if (bots.Contains(username.ToLower()))
                        {
                            // If it's not a police bot, return.
                            if (!from_policebot)
                                return;

                            String hashkey = Tools.md5((trigger + from + username).Replace(" ", "").ToLower());
                            bot.NPSay(ns, String.Format("BDS:BOTCHECK:RESPONSE:{0},{1},{2},{3}/{4},{5},{6}", from, owner, Program.BotName, Program.Version, Version, hashkey, trigger));
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

                        String hashkey = Tools.md5((trig + data[0] + from).ToLower().Replace(" ", "")).ToLower();

                        if (hashkey != hash)
                        {
                            // Invalid hash supplied
                            // For now, we ignore this. Though I'd like to see policebots send and error like:
                            //  BDS:BOTCHECK:ERROR:INVALID_RESPONSE_HASH
                            if (Program.Debug)
                                ConIO.Warning("BDS", "Invalid hash for bot: " + from);
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
                    else if (bits[2] == "CLIENT" && bits.Length >= 4)
                    {
                        // Look for a valid string
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
                        if (data.Length < 4)
                            return;

                        String name   = data[1];
                        String[] vers = data[2].Split('/');
                        String ver    = vers[0];
                        String hash   = data[3];

                        double bdsver = 0.2;

                        if (!Double.TryParse(vers[vers.Length - 1], out bdsver))
                            bdsver = 0.2;

                        if (vers.Length > 2)
                            ver = data[2].Substring(0, data[2].LastIndexOf('/'));

                        Types.ClientInfo client_info = new ClientInfo(from, name, ver, bdsver, Bot.EpochTimestamp);

                        String hashkey = Tools.md5((name + ver + from + data[0]).Replace(" ", "").ToLower()).ToLower();

                        if (hashkey != hash)
                        {
                            // Invalid hash supplied
                            // For now, we ignore this. Though I'd like to see policebots send and error like:
                            //  BDS:BOTCHECK:ERROR:INVALID_RESPONSE_HASH
                            if (Program.Debug)
                                ConIO.Warning("BDS", "Invalid hash for client: " + from);
                        }
                        else
                        {
                            lock (_clientinfo_database)
                            {
                                if (_clientinfo_database.ContainsKey(from.ToLower()))
                                {
                                    _clientinfo_database[from.ToLower()] = client_info;

                                    if (Program.Debug)
                                        ConIO.Write("Updated database for client: " + from, "BDS");
                                }
                                else
                                {
                                    _clientinfo_database.Add(from.ToLower(), client_info);

                                    if (Program.Debug)
                                        ConIO.Write("Added client to database: " + from, "BDS");
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
                    else if (bits.Length >= 4 && bits[2] == "CLIENTINFO")
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
                        if (data.Length < 3)
                            return;

                        String name = data[1];
                        String[] vers = data[2].Split('/');
                        String ver = vers[0];

                        double bdsver = 0.2;

                        if (!Double.TryParse(vers[vers.Length - 1], out bdsver))
                            bdsver = 0.2;

                        if (vers.Length > 2)
                            ver = data[2].Substring(0, data[2].LastIndexOf('/'));

                        Types.ClientInfo client_info = new ClientInfo(from, name, ver, bdsver, Bot.EpochTimestamp);

                        lock (_clientinfo_database)
                        {
                            if (_clientinfo_database.ContainsKey(data[0].ToLower()))
                            {
                                _clientinfo_database[data[0].ToLower()] = client_info;

                                if (Program.Debug)
                                    ConIO.Write("Updated database for client: " + data[0], "BDS");
                            }
                            else
                            {
                                _clientinfo_database.Add(data[0].ToLower(), client_info);

                                if (Program.Debug)
                                    ConIO.Write("Added client to database: " + data[0], "BDS");
                            }
                        }

                        lock (_info_requests)
                        {
                            if (_info_requests.ContainsKey(data[0].ToLower()))
                            {
                                String chan = _info_requests[data[0].ToLower()];
                                _info_requests.Remove(data[0].ToLower());

                                String output = String.Format("<b>&raquo; Information on :dev{0}:</b><br/>", client_info.Name);
                                output += String.Format("<b>Client type:</b> {0}<br/>", client_info.Type);
                                output += String.Format("<b>Client version:</b> {0}", client_info.Version);
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
                                bot.Say(chan, "<b>&raquo; Bot/client doesn't exist:</b> " + bits[3]);
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
                    else if (bits.Length >= 4 && bits[2] == "BADCLIENT")
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
                                bot.Say(chan, "<b>&raquo; Client is banned:</b> " + data[0]);
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
                        bot.NPSay(ns, String.Format("BDS:BOTDEF:RESPONSE:{0},{1},{2},{3},{4},{5}", from, Program.BotName, "C#", "DivinityArcane", "http://botdom.com/wiki/LulzBot", hashkey));
                    }
                }
                else if (bits.Length >= 4 && bits[1] == "TRANSLATE" && bits[2] == "RESPONSE")
                {
                    // Ignore data from non-police bots
                    if (!from_policebot)
                        return;

                    if (!bits[3].Contains(","))
                        return;

                    String input = String.Empty;

                    for (byte b = 3; b < bits.Length; b++)
                    {
                        if (b >= bits.Length - 1)
                            input += bits[b];
                        else
                            input += bits[b] + ":";
                    }

                    String[] data = input.Split(',');

                    if (data[0].ToLower() != username.ToLower() || data.Length < 3) return;

                    lock (_translate_requests)
                    {
                        if (_translate_requests.Contains(data[1]))
                        {
                            int id = _translate_requests.IndexOf(data[1]);
                            String chan = _translate_requests[id];
                            _translate_requests.RemoveAt(id);
                            bot.Say(chan, "<b>&raquo; Translated text:</b> " + WebUtility.HtmlEncode(Encoding.UTF8.GetString(Convert.FromBase64String(data[2]))));
                        }
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
                                String[] pars = bits[3].Split(new char[] { ',' });
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
