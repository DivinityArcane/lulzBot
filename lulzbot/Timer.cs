using System;
using System.Collections.Generic;
using System.Timers;

namespace lulzbot
{
    public class Timers
    {
        private static Dictionary<String, Timer> timers = new Dictionary<String, Timer>();

        public static int Count
        {
            get
            {
                return timers.Count;
            }
        }

        public static String Add (int delay, ElapsedEventHandler action)
        {
            String id = Tools.md5(String.Format("{0}", Environment.TickCount + timers.Count));
            Timer t = new Timer(delay);
            t.Elapsed += action;
            t.Elapsed += delegate
            {
                t.Stop();
                Remove(id);
            };
            timers.Add(id, t);
            t.Start();
            return id;
        }

        public static bool Remove (String id)
        {
            if (timers.ContainsKey(id))
            {
                timers[id].Dispose();
                return timers.Remove(id);
            }
            return false;
        }
    }
}
