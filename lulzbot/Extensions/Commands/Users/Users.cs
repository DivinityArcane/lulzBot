using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Linq;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Users
    {
        public static void cmd_users(Bot bot, String ns, String[] args, String msg, String from, dAmnPacket packet)
        {
            String helpmsg = String.Format("<b>&raquo; Usage:</b><br/>{0}users list<br/>{0}users add username <i>privs</i><br/>{0}users del username", " &middot; " + bot.Config.Trigger);

            if (args.Length >= 2)
            {
                String arg = args[1];

                if (arg == "list")
                {
                    var Output              = String.Empty;
                    List<String> Admins     = new List<String>();
                    List<String> Operators  = new List<String>();
                    List<String> Members    = new List<String>();
                    List<String> Banned     = new List<String>();

                    foreach (UserData user in userdata.Values)
                    {
                        if (user.PrivLevel == (int)Privs.Admins) Admins.Add(user.Name);
                        else if (user.PrivLevel == (int)Privs.Operators) Operators.Add(user.Name);
                        else if (user.PrivLevel == (int)Privs.Members) Members.Add(user.Name);
                        else if (user.PrivLevel == (int)Privs.Banned) Banned.Add(user.Name);
                    }

                    Admins.Sort();
                    Operators.Sort();
                    Members.Sort();
                    Banned.Sort();

                    Output += String.Format("<b>Owner:</b><br/><b>&raquo;</b> :dev{0}:<br/>", bot.Config.Owner);
                    Output += String.Format("<br/><b>Admins:</b><br/><b>&raquo;</b> {0}<br/>", (Admins.Count > 0 ? String.Join(", ", Admins) : "None"));
                    Output += String.Format("<br/><b>Operators:</b><br/><b>&raquo;</b> {0}<br/>", (Operators.Count > 0 ? String.Join(", ", Operators) : "None"));
                    Output += String.Format("<br/><b>Members:</b><br/><b>&raquo;</b> {0}<br/>", (Members.Count > 0 ? String.Join(", ", Members) : "None"));
                    Output += String.Format("<br/><b>Banned:</b><br/><b>&raquo;</b> {0}<br/>", (Banned.Count > 0 ? String.Join(", ", Banned) : "None"));

                    bot.Say(ns, Output);
                }
                else if (args.Length >= 3)
                {
                    if (args[1] == "del")
                    {
                        String who = args[2].ToLower();

                        if (userdata.ContainsKey(who))
                        {
                            String realname = userdata[who].Name;
                            userdata.Remove(who);
                            Storage.Save("users", userdata);
                            bot.Say(ns, String.Format("<b>&raquo; User removed:</b> :dev{0}:", realname));
                        }
                        else
                        {
                            bot.Say(ns, String.Format("<b>&raquo; No such user:</b> {0}", who));
                        }
                    }
                    else if (args.Length >= 4 && args[1] == "add")
                    {
                        String who = args[2].ToLower();
                        String privs = args[3].ToLower();

                        if (userdata.ContainsKey(who))
                        {
                            String realname = userdata[who].Name;
                            bot.Say(ns, String.Format("<b>&raquo; User already exists:</b> :dev{0}:", realname));
                        }
                        else
                        {
                            UserData data = new UserData()
                            {
                                Name = args[2],
                                Access = new List<String>(),
                                Banned = new List<String>()
                            };

                            if (privs == "owner")
                            {
                                bot.Say(ns, "<b>&raquo; Adding other users as owners is not allowed!</b>");
                                return;
                            }
                            else if (privs == "admins") data.PrivLevel = (int)Privs.Admins;
                            else if (privs == "operators") data.PrivLevel = (int)Privs.Operators;
                            else if (privs == "members") data.PrivLevel = (int)Privs.Members;
                            else if (privs == "banned") data.PrivLevel = (int)Privs.Banned;
                            else
                            {
                                bot.Say(ns, "<b>&raquo; Invalid privilege level! Correct values:</b> Admins, Operators, Members, Banned.</b>");
                                return;
                            }

                            userdata.Add(who, data);
                            Storage.Save("users", userdata);

                            bot.Say(ns, String.Format("<b>&raquo; Added user:</b> :dev{0}:", who));
                        }
                    }
                    else
                    {
                        bot.Say(ns, helpmsg);
                    }
                }
                else
                {
                    bot.Say(ns, helpmsg);
                }
            }
            else
            {
                bot.Say(ns, helpmsg);
            }
        }
    }
}

