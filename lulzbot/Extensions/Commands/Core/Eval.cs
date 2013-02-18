using lulzbot.Networking;
using lulzbot.Types;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace lulzbot.Extensions
{
    public partial class Core
    {
        public static void cmd_eval(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            if (args.Length < 2)
            {
                bot.Say(ns, String.Format("<b>&raquo; Usage:</b> {0}eval code", bot.Config.Trigger));
            }
            else
            {
                try
                {
                    String code = "using System;\n" +
                        "using System.Linq;\n" +
                        "using System.Linq.Expressions;\n" +
                        "using System.Collections;\n" +
                        "using System.Collections.Generic;\n" +
                        "using lulzbot;\n" +
                        "using lulzbot.Extensions;\n\n" +
                        "public class c_eval {\n\t#pragma warning disable 162\n\t" +
                        "public static object v_eval () {\n\t\t" +
                        msg.Substring(5) + "\n\t\treturn null;\n\t}\n}";

                    CodeDomProvider codeDomProvider = CSharpCodeProvider.CreateProvider("C#");
                    CompilerParameters compilerParams = new CompilerParameters();
                    compilerParams.GenerateExecutable = false;
                    compilerParams.GenerateInMemory = true;
                    compilerParams.IncludeDebugInformation = false;
                    compilerParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                    CompilerResults results = codeDomProvider.CompileAssemblyFromSource(compilerParams, code);

                    if (results.Errors.Count > 0)
                    {
                        foreach (CompilerError error in results.Errors)
                        {
                            bot.Say(ns, "<b>&raquo; Error in code:</b> " + error.ToString());
                        }
                    }
                    else
                    {
                        try
                        {
                            var compiled_type = results.CompiledAssembly.GetType("c_eval");
                            var method = compiled_type.GetMethod("v_eval");
                            object res = method.Invoke(null, null);
                            if (res != null)
                                bot.Say(ns, String.Format("<b>&raquo; Output:</b><bcode>{0}</bcode>", res.ToString()));
                        }
                        catch (Exception E)
                        {
                            bot.Say(ns, "<b>&raquo; Error while executing code:</b> " + E.ToString());
                        }
                    }
                }
                catch (Exception E)
                {
                    bot.Say(ns, "<b>&raquo; Unable to evaluate code:</b> " + E.ToString());
                }
            }
        }
    }
}

