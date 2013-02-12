﻿using lulzbot.Networking;
using lulzbot.Types;
using System;
using System.Collections.Generic;

namespace lulzbot.Extensions
{
    public partial class Users
    {
        public static Dictionary<String, UserData> userdata = new Dictionary<String, UserData>();

        /// <summary>
        /// Constructor. Add basic events.
        /// </summary>
        public Users(String owner)
        {
            Events.AddCommand("users",  new Command(this, "cmd_users",  "DivinityArcane", 75, "Manages bot users."));
            Events.AddCommand("access", new Command(this, "cmd_access", "DivinityArcane", 75, "Manages individual command access."));

            userdata = Storage.Load<Dictionary<String, UserData>>("users");

            if (userdata == null || !userdata.ContainsKey(owner.ToLower()))
            {
                userdata = new Dictionary<String, UserData>();
                userdata.Add(owner.ToLower(), new UserData()
                {
                    Name        = owner,
                    PrivLevel   = 100,
                    Access      = new List<String>(),
                    Banned      = new List<String>()
                });
                Storage.Save("users", userdata);
            }
        }

        public static bool CanAccess(String username, int privs, String cmd)
        {
            int pl = 25;
            String who = username.ToLower();

            if (userdata.ContainsKey(who))
            {
                pl = userdata[who].PrivLevel;

                if (userdata[who].Access.Contains(cmd.ToLower())) return true;

                if (userdata[who].Banned.Contains(cmd.ToLower())) return false;
            }

            return pl >= privs;
        }

        public static int GetPrivs(String username)
        {
            int pl = 25;
            if (userdata.ContainsKey(username.ToLower())) pl = userdata[username.ToLower()].PrivLevel;
            return pl;
        }
    }
}
