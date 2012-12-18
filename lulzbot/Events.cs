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

            _events.Add("on_connect",   new List<Event>());
            _events.Add("dAmnServer",   new List<Event>());
            _events.Add("disconnect",   new List<Event>());
            _events.Add("login",        new List<Event>());
            _events.Add("join",         new List<Event>());
            _events.Add("part",         new List<Event>());
            _events.Add("get",          new List<Event>());
            _events.Add("set",          new List<Event>());
            _events.Add("send",         new List<Event>());
            _events.Add("kick",         new List<Event>());
            _events.Add("kicked",       new List<Event>());
            _events.Add("kill",         new List<Event>());
            _events.Add("ping",         new List<Event>());
            _events.Add("whois",        new List<Event>());
            _events.Add("property",     new List<Event>());
            _events.Add("recv_action",  new List<Event>());
            _events.Add("recv_msg",     new List<Event>());
            _events.Add("recv_join",    new List<Event>());
            _events.Add("recv_part",    new List<Event>());
            _events.Add("recv_admin",   new List<Event>());
            _events.Add("recv_kicked",  new List<Event>());
            _events.Add("recv_privchg", new List<Event>());

            /// TODO: Maybe add some bot-related events for use later?

            ConIO.Write(String.Format("Initialized {0} events.", _events.Count));
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
                Command callback = _commands[cmd_name];
                callback.Method.Invoke(callback.Class, new object[] { Program.Bot, packet.Parameter, packet.Body, packet });
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
