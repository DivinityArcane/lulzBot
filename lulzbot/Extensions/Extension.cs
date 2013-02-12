using lulzbot.Types;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace lulzbot.Extensions
{
    public class ExtensionContainer
    {
        public static String CurrentFile = String.Empty;

        public ExtensionContainer()
        {
            Load();
        }

        public void Load()
        {
            if (!Directory.Exists("./Extensions/Enabled"))
            {
                // If it doesn't exist, there's no extensions. Create and leave.
                Directory.CreateDirectory("./Extensions/Enabled");
                return;
            }

            Events.ClearExternalEvents();

            String[] files = Directory.GetFiles("./Extensions/Enabled", "*.cs");

            foreach (String file in files)
            {
                LoadFile(file);
            }
        }

        private void LoadFile(String filename)
        {
            CurrentFile = Path.GetFileName(filename);

            CodeDomProvider codeDomProvider         = CSharpCodeProvider.CreateProvider("C#");
            CompilerParameters compilerParams       = new CompilerParameters();
            compilerParams.GenerateExecutable       = false;
            compilerParams.GenerateInMemory         = true;
            compilerParams.IncludeDebugInformation  = false;

            compilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("Newtonsoft.Json.dll");

            CompilerResults results = codeDomProvider.CompileAssemblyFromFile(compilerParams, filename);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                {
                    ConIO.Warning("Extension.LoadFile[Compilation error]", error.ToString());
                }
            }
            else
            {
                try
                {
                    var compiled_type = results.CompiledAssembly.GetType("Extension");
                    System.Attribute[] attrs = System.Attribute.GetCustomAttributes(compiled_type);
                    ExtensionInfo ext_info = null;
                    if (attrs.Length == 0 || (ext_info = attrs[0] as ExtensionInfo) == null)
                    {
                        ConIO.Warning("Extensions", "No valid ExtensionInfo attribute: " + filename);
                    }
                    else
                    {
                        ConIO.Write (String.Format("Loaded extension: {0} v{1} by {2}.", ext_info.Name, ext_info.Version, ext_info.Author));
                        object class_instance = Activator.CreateInstance(compiled_type);

                        foreach (MethodInfo method in compiled_type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (Attribute.IsDefined(method, typeof(BindCommand)))
                            {
                                object[] m_attrs = method.GetCustomAttributes(true);
                                foreach (object potential in m_attrs)
                                {
                                    BindCommand cmd_info = potential as BindCommand;
                                    if (cmd_info != null)
                                    {
                                        Events.AddExternalCommand(cmd_info.Command, new Command(class_instance, method.Name, ext_info.Author, cmd_info.Privileges, cmd_info.Description));
                                    }
                                }
                            }
                            else if (Attribute.IsDefined(method, typeof(BindEvent)))
                            {
                                
                                object[] m_attrs = method.GetCustomAttributes(true);
                                foreach (object potential in m_attrs)
                                {
                                    BindEvent evt_info = potential as BindEvent;
                                    if (evt_info != null)
                                    {
                                        Events.AddExternalEvent(evt_info.Event, new Event(class_instance, method.Name, evt_info.Description, ext_info.Name));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                    ConIO.Warning("Extension.LoadFile[Execution error]", E.ToString());
                }
            }

            CurrentFile = String.Empty;
        }
    }

    /// <summary>
    /// Static class for external extensions. Gives an easy way to do most things with a friendlier interface
    /// </summary>
    public class LulzBot
    {
        public static String Trigger = Program.Bot.Config.Trigger;
        public static String Username = Program.Bot.Config.Username;
        public static String Owner = Program.Bot.Config.Owner;

        public static void Print(String msg)
        {
            ConIO.Write(msg);
        }

        public static void Say(String chan, String msg)
        {
            Program.Bot.Say(chan, msg);
        }

        public static void Act(String chan, String msg)
        {
            Program.Bot.Act(chan, msg);
        }

        public static void Join(String chan)
        {
            Program.Bot.Join(chan);
        }

        public static void Part(String chan)
        {
            Program.Bot.Part(chan);
        }

        public static void AddEvent(String event_name, object class_ref, String method_name)
        {
            if (ExtensionContainer.CurrentFile == String.Empty)
                throw new Exception("Events must be bound in the Initialize() method!");

            if (!Events.ValidateName(event_name))
                throw new Exception("Invalid event name: " + event_name);

            Event evt = new Event(class_ref, method_name, "External event bound to " + event_name, String.Format("Extension[{0}]", ExtensionContainer.CurrentFile));
            Events.AddExternalEvent(event_name, evt);
        }

        public static void AddCommand(String cmd_name, object class_ref, String method_name, String author, byte privs, String desc)
        {
            if (ExtensionContainer.CurrentFile == String.Empty)
                throw new Exception("Events must be bound in the Initialize() method!");

            if (Events.CommandExists(cmd_name))
                throw new Exception("Command name is already taken: " + cmd_name.ToLower());

            Command cmd = new Command(class_ref, method_name, author, privs, desc);
            Events.AddExternalCommand(cmd_name, cmd);
        }

        public static void Save(String filename, object data)
        {
            Storage.Save(filename, data);
        }

        public static T Load<T>(String filename)
        {
            T data = Storage.Load<T>(filename);
            return data;
        }
    }
}
