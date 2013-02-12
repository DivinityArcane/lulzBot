using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static Dictionary<String, Types.ChatData> ChannelData = new Dictionary<String, Types.ChatData>();

        /// <summary>
        /// Keeps track of which channels certain commands were sent from.
        /// </summary>
        public static Dictionary<String, List<String>> CommandChannels = new Dictionary<String, List<String>>();

        /// <summary>
        /// Constructor. Add basic events.
        /// </summary>
        public Core()
        {
            // Format is simple:
            // Events.AddEvent("event_name", event_obj);
            // Where event_obj is:
            //  new Event(this, "function_name")
            // To keep things clean, start function names for events with evt_
            Events.AddEvent("on_connect",   new Event(this, "evt_connect"));
            Events.AddEvent("dAmnServer",   new Event(this, "evt_preauth"));
            Events.AddEvent("login",        new Event(this, "evt_login"));
            Events.AddEvent("join",         new Event(this, "evt_join"));
            Events.AddEvent("part",         new Event(this, "evt_part"));
            Events.AddEvent("property",     new Event(this, "evt_property"));
            Events.AddEvent("recv_msg",     new Event(this, "evt_recv_msg"));
            Events.AddEvent("recv_action",  new Event(this, "evt_recv_action"));
            Events.AddEvent("recv_join",    new Event(this, "evt_recv_join"));
            Events.AddEvent("recv_part",    new Event(this, "evt_recv_part"));
            Events.AddEvent("recv_privchg", new Event(this, "evt_recv_privchg"));
            Events.AddEvent("recv_kicked",  new Event(this, "evt_recv_kicked"));
            Events.AddEvent("recv_admin",   new Event(this, "evt_recv_admin"));
            Events.AddEvent("kicked",       new Event(this, "evt_kicked"));
            Events.AddEvent("disconnect",   new Event(this, "evt_disconnect"));
            Events.AddEvent("send",         new Event(this, "evt_send_error"));
            Events.AddEvent("kick",         new Event(this, "evt_kick_error"));
            Events.AddEvent("get",          new Event(this, "evt_get_error"));
            Events.AddEvent("set",          new Event(this, "evt_set_error"));
            Events.AddEvent("kill",         new Event(this, "evt_kill_error"));
            Events.AddEvent("ping",         new Event(this, "evt_ping"));

            // Again, simple format:
            // Events.AddCommand("command_name", cmd_obj);
            // Where cmd_obj is:
            //  new Command(this, "function_name", "Your_dA_username", "A simple help msg.", minimum_privs, "Description")
            // minimum_privs = minimum privilege level. 25 = guests, 50 = members, 75 = opers, 99 = admins, 100 = owner.
            Events.AddCommand("about",      new Command(this, "cmd_about",            "DivinityArcane", 25,     "Displays information about the bot."));
            Events.AddCommand("act",        new Command(this, "cmd_act",              "DivinityArcane", 75,     "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("admin",      new Command(this, "cmd_admin",            "DivinityArcane", 75,     "Makes the bot send the specified admin command to the specified channel."));
            Events.AddCommand("ban",        new Command(this, "cmd_ban",              "DivinityArcane", 75,     "Bans the specified user in the specified channel."));
            Events.AddCommand("commands",   new Command(this, "cmd_commands",         "DivinityArcane", 25,     "Displays commands available to the user."));
            Events.AddCommand("demote",     new Command(this, "cmd_demote",           "DivinityArcane", 75,     "Demotes the specified user in the specified channel."));
            Events.AddCommand("event",      new Command(this, "cmd_event",            "DivinityArcane", 25,     "Gets information on the events system."));
            Events.AddCommand("get",        new Command(this, "cmd_get",              "DivinityArcane", 50,     "Gets the specified data for the specified channel."));
            Events.AddCommand("join",       new Command(this, "cmd_join",             "DivinityArcane", 75,     "Makes the bot join the specified channel."));
            Events.AddCommand("kick",       new Command(this, "cmd_kick",             "DivinityArcane", 75,     "Makes the bot kick the specified person in the specified channel."));
            Events.AddCommand("kill",       new Command(this, "cmd_kill",             "DivinityArcane", 75,     "Makes the bot kill the specified person."));
            Events.AddCommand("netinfo",    new Command(this, "cmd_netinfo",          "DivinityArcane", 25,     "Gets information on the network usage of the bot."));
            Events.AddCommand("npsay",      new Command(this, "cmd_npsay",            "DivinityArcane", 75,     "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("part",       new Command(this, "cmd_part",             "DivinityArcane", 75,     "Makes the bot leave the specified channel."));
            Events.AddCommand("ping",       new Command(this, "cmd_ping",             "DivinityArcane", 25,     "Tests the latency between the bot and the server."));
            Events.AddCommand("promote",    new Command(this, "cmd_promote",          "DivinityArcane", 75,     "Promotes the specified user in the specified channel."));
            Events.AddCommand("quit",       new Command(this, "cmd_quit",             "DivinityArcane", 100,    "Closes the bot down gracefully."));
            Events.AddCommand("reload",     new Command(this, "cmd_reload",           "DivinityArcane", 100,    "Reloads external commands."));
            Events.AddCommand("say",        new Command(this, "cmd_say",              "DivinityArcane", 75,     "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("set",        new Command(this, "cmd_set",              "DivinityArcane", 75,     "Sets the specified data for the specified channel."));
            Events.AddCommand("sudo",       new Command(this, "cmd_sudo",             "DivinityArcane", 100,    "Runs a command as the specified user."));
            Events.AddCommand("unban",      new Command(this, "cmd_unban",            "DivinityArcane", 75,     "Un-bans the specified user in the specified channel."));
            Events.AddCommand("uptime",     new Command(this, "cmd_uptime",           "DivinityArcane", 25,     "Returns how long the bot has been running."));

            // Initialize CommandsChannels
            String[] c_types = new String[] { "join", "part", "send", "set", "kick", "kill", "promote", "demote", "admin" };
            CommandChannels.Clear();

            foreach (String c_type in c_types)
                CommandChannels.Add(c_type, new List<String>());
        }
    }
}
