using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Whois(String who)
        {
            return Encoding.ASCII.GetBytes(String.Format("get login:{0}\np=info\n\0", who));
        }
    }
}
