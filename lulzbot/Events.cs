using lulzbot.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace lulzbot
{
    /// <summary>
    /// This is our event system.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// This is the events dictionary. Each event is a list of Event objects.
        /// </summary>
        private static Dictionary<String, List<Event>> _events = new Dictionary<string, List<Event>>();
        private static Dictionary<String, Command> _commands = new Dictionary<string, Command>();

        /// <summary>
        /// Adds the default event names and lists.
        /// </summary>
        public static void InitEvents()
        {
            ConIO.Write("Initializing events...");

            // dAmn events
            AddEventType("dAmnServer");
            AddEventType("disconnect");
            AddEventType("get");
            AddEventType("join");
            AddEventType("kick");
            AddEventType("kicked");
            AddEventType("kill");
            AddEventType("login");
            AddEventType("on_connect");
            AddEventType("part");
            AddEventType("ping");
            AddEventType("property");
            AddEventType("recv_action");
            AddEventType("recv_admin");
            AddEventType("recv_join");
            AddEventType("recv_kicked");
            AddEventType("recv_msg");
            AddEventType("recv_part");
            AddEventType("recv_privchg");
            AddEventType("send");
            AddEventType("set");
            AddEventType("whois");

            // non dAmn events

            AddEventType("log_msg");

            ConIO.Write(String.Format("Initialized {0} events.", _events.Count));
        }

        /// <summary>
        /// Adds a new event list for the specified event name.
        /// </summary>
        /// <param name="event_name">Event name. i.e. recv_msg, do_something</param>
        private static void AddEventType(String event_name)
        {
            if (!_events.ContainsKey(event_name))
                _events.Add(event_name, new List<Event>());
        }

        /// <summary>
        /// Adds an event to the list.
        /// </summary>
        /// <param name="event_name">name of the event</param>
        /// <param name="callback">Event object</param>
        public static void AddEvent(String event_name, Event callback)
        {
            if (_events.ContainsKey(event_name))
            {
                _events[event_name].Add(callback);
            }
            else
            {
                ConIO.Write("Invalid event: " + event_name, "Events");
            }
        }

        /// <summary>
        /// Calls all the events bound to the specified event name
        /// </summary>
        /// <param name="event_name">event name</param>
        /// <param name="packet">dAmnPacket object</param>
        public static void CallEvent(String event_name, dAmnPacket packet)
        {
            if (_events.ContainsKey(event_name))
            {
                foreach (Event callback in _events[event_name])
                {
                    callback.Method.Invoke(callback.Class, new object[] { Program.Bot, packet });
                }
            }
            else
            {
                ConIO.Write("Unknown event: " + event_name, "Events");
            }
        }

        /// <summary>
        /// Calls all the events bound to the specified event name
        /// (For non dAmn events)
        /// </summary>
        /// <param name="event_name">event name</param>
        /// <param name="parameters">list of parameters to be passed to the events</param>
        public static void CallSpecialEvent(String event_name, object[] parameters)
        {
            if (_events.ContainsKey(event_name))
            {
                foreach (Event callback in _events[event_name])
                {
                    callback.Method.Invoke(callback.Class, parameters);
                }
            }
            else
            {
                ConIO.Write("Unknown special event: " + event_name, "Events");
            }
        }

        /// <summary>
        /// Adds a command to the list.
        /// </summary>
        /// <param name="cmd_name">name of the command</param>
        /// <param name="callback">Command object</param>
        public static void AddCommand(String cmd_name, Command callback)
        {
            if (!_commands.ContainsKey(cmd_name))
            {
                _commands[cmd_name] = callback;
            }
            else
            {
                ConIO.Write("Duplicate command: " + cmd_name, "Events");
            }
        }

        /// <summary>
        /// Calls the command bound to the specified command name
        /// </summary>
        /// <param name="cmd_name">command name</param>
        public static void CallCommand(String cmd_name, dAmnPacket packet)
        {
            if (_commands.ContainsKey(cmd_name))
            {
                // Replace with a user check later
                int my_privs = 25;
                Command callback = _commands[cmd_name];
                String from = String.Empty;
                if (packet.Arguments.ContainsKey("from"))
                    from = packet.Arguments["from"];

                if (from.ToLower() == Program.Bot.Config.Owner.ToLower())
                    my_privs = 100;

                // Access denied
                if (callback.MinimumPrivs > my_privs)
                    return;

                callback.Method.Invoke(callback.Class, new object[] { Program.Bot, packet.Parameter, packet.Body, from, packet });
            }
            else
            {
                ConIO.Write("Unknown command: " + cmd_name, "Events");
            }
        }

        /// <summary>
        /// Clears all the events.
        /// </summary>
        public static void ClearEvents()
        {
            foreach (String event_name in _events.Keys)
            {
                _events[event_name].Clear();
            }
        }

        /// <summary>
        /// Returns a list of all commands accessable to privlevel minimum_priv_level
        /// </summary>
        /// <param name="minimum_priv_level">Minimum privilege level</param>
        /// <returns>Sorted list of command names</returns>
        public static List<String> GetAvailableCommands(int minimum_priv_level)
        {
            List<String> list = new List<string>();

            foreach (KeyValuePair<String, Command> KVP in _commands)
            {
                if (KVP.Value.MinimumPrivs <= minimum_priv_level)
                    list.Add(KVP.Key);
            }

            list.Sort();

            return list;
        }
    }

    /// <summary>
    /// Event object
    /// </summary>
    public class Event 
    {
        /// <summary>
        /// Class's "this" object
        /// </summary>
        public object Class;

        /// <summary>
        /// Info on the method we will call
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// Description of the event
        /// </summary>
        public String Description = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="class_obj">Class pointer (i.e. "this")</param>
        /// <param name="method_name">Callback method name</param>
        /// <param name="desc">Description</param>
        public Event(object class_obj, String method_name, String desc = "")
        {
            Class = class_obj;
            Method = Class.GetType().GetMethod(method_name);
            Description = desc;
        }
    }


    /// <summary>
    /// Command object
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Class's "this" object
        /// </summary>
        public object Class;

        /// <summary>
        /// Information on the method we will call.
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// Description of the command.
        /// </summary>
        public String Description = String.Empty;

        /// <summary>
        /// Default help message. {trig} is replaced by the bot's trigger.
        /// </summary>
        public String Help = String.Empty;

        /// <summary>
        /// Author's dA username
        /// </summary>
        public String Author = String.Empty;

        /// <summary>
        /// Minimum privilege level. Default: 25 (Owner: 100, Admins: 99, Operators: 75, Members: 50, Guests: 25)
        /// </summary>
        public int MinimumPrivs = 25;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="class_obj">Class pointer (i.e. "this")</param>
        /// <param name="method_name">Callback method name</param>
        /// <param name="desc">Description</param>
        public Command(object class_obj, String method_name, 
            String author = "", String help = "", int privs = 25, String desc = "")
        {
            Class = class_obj;
            Method = Class.GetType().GetMethod(method_name);
            Author = author;
            Help = help;
            Description = desc;
            MinimumPrivs = privs;
        }
    }
}
