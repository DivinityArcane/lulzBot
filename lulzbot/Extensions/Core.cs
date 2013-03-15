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

        public static List<String> _disabled_commands;

        /// <summary>
        /// Constructor. Add basic events.
        /// </summary>
        public Core ()
        {
            Events.AddEvent("on_connect", new Event(this, "evt_connect"));
            Events.AddEvent("dAmnServer", new Event(this, "evt_preauth"));
            Events.AddEvent("login", new Event(this, "evt_login"));
            Events.AddEvent("join", new Event(this, "evt_join"));
            Events.AddEvent("part", new Event(this, "evt_part"));
            Events.AddEvent("property", new Event(this, "evt_property"));
            Events.AddEvent("recv_msg", new Event(this, "evt_recv_msg"));
            Events.AddEvent("recv_action", new Event(this, "evt_recv_action"));
            Events.AddEvent("recv_join", new Event(this, "evt_recv_join"));
            Events.AddEvent("recv_part", new Event(this, "evt_recv_part"));
            Events.AddEvent("recv_privchg", new Event(this, "evt_recv_privchg"));
            Events.AddEvent("recv_kicked", new Event(this, "evt_recv_kicked"));
            Events.AddEvent("recv_admin", new Event(this, "evt_recv_admin"));
            Events.AddEvent("kicked", new Event(this, "evt_kicked"));
            Events.AddEvent("disconnect", new Event(this, "evt_disconnect"));
            Events.AddEvent("send", new Event(this, "evt_send_error"));
            Events.AddEvent("kick", new Event(this, "evt_kick_error"));
            Events.AddEvent("get", new Event(this, "evt_get_error"));
            Events.AddEvent("set", new Event(this, "evt_set_error"));
            Events.AddEvent("kill", new Event(this, "evt_kill_error"));
            Events.AddEvent("ping", new Event(this, "evt_ping"));

            Events.AddCommand("about", new Command(this, "cmd_about", "DivinityArcane", 25, "Displays information about the bot."));
            Events.AddCommand("autojoin", new Command(this, "cmd_autojoin", "DivinityArcane", 100, "Manages the bots autojoined channels."));
            Events.AddCommand("act", new Command(this, "cmd_act", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("admin", new Command(this, "cmd_admin", "DivinityArcane", 75, "Makes the bot send the specified admin command to the specified channel."));
            Events.AddCommand("ban", new Command(this, "cmd_ban", "DivinityArcane", 75, "Bans the specified user in the specified channel."));
            Events.AddCommand("chat", new Command(this, "cmd_chat", "DivinityArcane", 75, "Makes the bot join a private chat."));
            Events.AddCommand("channels", new Command(this, "cmd_channels", "DivinityArcane", 50, "Displays the channels the bot has joined."));
            Events.AddCommand("cycle", new Command(this, "cmd_cycle", "DivinityArcane", 75, "Makes the bot part and join a channel."));
            Events.AddCommand("command", new Command(this, "cmd_command", "DivinityArcane", 100, "Disables certain commands."));
            Events.AddCommand("commands", new Command(this, "cmd_commands", "DivinityArcane", 25, "Displays commands available to the user."));
            Events.AddCommand("ctrig", new Command(this, "cmd_ctrig", "DivinityArcane", 100, "Changes the bot's trigger."));
            Events.AddCommand("debug", new Command(this, "cmd_debug", "DivinityArcane", 100, "Toggles debug mode."));
            Events.AddCommand("demote", new Command(this, "cmd_demote", "DivinityArcane", 75, "Demotes the specified user in the specified channel."));
            Events.AddCommand("disconnects", new Command(this, "cmd_disconnects", "DivinityArcane", 25, "Displays how many times the bot has disconnected since startup."));
            Events.AddCommand("exec", new Command(this, "cmd_exec", "DivinityArcane", 100, "Executes a system command."));
            Events.AddCommand("eval", new Command(this, "cmd_eval", "DivinityArcane", 100, "Evaluates C# code."));
            Events.AddCommand("event", new Command(this, "cmd_event", "DivinityArcane", 25, "Gets information on the events system."));
            Events.AddCommand("get", new Command(this, "cmd_get", "DivinityArcane", 50, "Gets the specified data for the specified channel."));
            Events.AddCommand("help", new Command(this, "cmd_help", "DivinityArcane", 25, "Checks the description of the specified command."));
            Events.AddCommand("join", new Command(this, "cmd_join", "DivinityArcane", 75, "Makes the bot join the specified channel."));
            Events.AddCommand("kick", new Command(this, "cmd_kick", "DivinityArcane", 75, "Makes the bot kick the specified person in the specified channel."));
            Events.AddCommand("kill", new Command(this, "cmd_kill", "DivinityArcane", 75, "Makes the bot kill the specified person."));
            Events.AddCommand("netusage", new Command(this, "cmd_netinfo", "DivinityArcane", 25, "Gets information on the network usage of the bot."));
            Events.AddCommand("netinfo", new Command(this, "cmd_netinfo", "DivinityArcane", 25, "Gets information on the network usage of the bot."));
            Events.AddCommand("npsay", new Command(this, "cmd_npsay", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("part", new Command(this, "cmd_part", "DivinityArcane", 75, "Makes the bot leave the specified channel."));
            Events.AddCommand("ping", new Command(this, "cmd_ping", "DivinityArcane", 25, "Tests the latency between the bot and the server."));
            Events.AddCommand("promote", new Command(this, "cmd_promote", "DivinityArcane", 75, "Promotes the specified user in the specified channel."));
            Events.AddCommand("quit", new Command(this, "cmd_quit", "DivinityArcane", 100, "Closes the bot down gracefully."));
            Events.AddCommand("reload", new Command(this, "cmd_reload", "DivinityArcane", 100, "Reloads external commands."));
            Events.AddCommand("say", new Command(this, "cmd_say", "DivinityArcane", 75, "Makes the bot say the specified message to the specified channel."));
            Events.AddCommand("set", new Command(this, "cmd_set", "DivinityArcane", 75, "Sets the specified data for the specified channel."));
            Events.AddCommand("sudo", new Command(this, "cmd_sudo", "DivinityArcane", 100, "Runs a command as the specified user."));
            Events.AddCommand("system", new Command(this, "cmd_system", "DivinityArcane", 25, "Gets information on the host machine."));
            Events.AddCommand("unban", new Command(this, "cmd_unban", "DivinityArcane", 75, "Un-bans the specified user in the specified channel."));
            Events.AddCommand("update", new Command(this, "cmd_update", "DivinityArcane", 25, "Checks if the bot is up to date."));
            Events.AddCommand("uptime", new Command(this, "cmd_uptime", "DivinityArcane", 25, "Returns how long the bot has been running."));
            Events.AddCommand("whois", new Command(this, "cmd_whois", "DivinityArcane", 25, "Performs a whois on the specified user."));

            String[] c_types = new String[] { "join", "part", "send", "set", "kick", "kill", "promote", "demote", "admin", "whois" };
            CommandChannels.Clear();

            foreach (String c_type in c_types)
                CommandChannels.Add(c_type, new List<String>());

            _disabled_commands = Storage.Load<List<String>>("disabled_commands");

            if (_disabled_commands == null)
                _disabled_commands = new List<String>();
        }

        private static void SaveDisabled ()
        {
            Storage.Save("disabled_commands", _disabled_commands);
        }
    }
}
