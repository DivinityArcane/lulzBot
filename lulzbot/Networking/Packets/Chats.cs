﻿using System;
using System.Text;

namespace lulzbot.Networking
{
    public partial class dAmnPackets
    {
        public static byte[] Join(String channel)
        {
            return Encoding.ASCII.GetBytes(String.Format("join {0}\n\0", channel));
        }

        public static byte[] Part(String channel)
        {
            return Encoding.ASCII.GetBytes(String.Format("part {0}\n\0", channel));
        }

        public static byte[] Message(String channel, String message)
        {
            return Encoding.ASCII.GetBytes(String.Format("send {0}\n\nmsg main\n\n{1}\n\0", channel, message));
        }

        public static byte[] NPMessage(String channel, String message)
        {
            return Encoding.ASCII.GetBytes(String.Format("send {0}\n\nnpmsg main\n\n{1}\n\0", channel, message));
        }

        public static byte[] Action(String channel, String message)
        {
            return Encoding.ASCII.GetBytes(String.Format("send {0}\n\naction main\n\n{1}\n\0", channel, message));
        }
    }
}