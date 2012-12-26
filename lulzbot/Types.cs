using System;
using System.Collections.Generic;

namespace lulzbot.Types
{

    #region BDS related
    /// <summary>
    /// BotDef object. Holds information on a bot type definition.
    /// </summary>
    public class BotDef
    {
        public String Name      = String.Empty;
        public String Author    = String.Empty;
        public String Language  = String.Empty;
        public String Link      = String.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bot type</param>
        /// <param name="author">Bot's author</param>
        /// <param name="language">Language the bot is programmed in</param>
        /// <param name="link">Link to the bot's wiki page</param>
        public BotDef(String name, String author, String language, String link)
        {
            Name        = name;
            Author      = author;
            Language    = language;
            Link        = link;
        }
    }

    /// <summary>
    /// BotInfo object. Holds information on a specific bot.
    /// </summary>
    public class BotInfo
    {
        public String Name          = String.Empty;
        public String Owner         = String.Empty;
        public String Type          = String.Empty;
        public String Version       = String.Empty;
        public String Trigger       = String.Empty;
        public double BDSVersion    = 0.0;
        public int Modified         = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Bot username</param>
        /// <param name="owner">Bot owner's username</param>
        /// <param name="type">Bot type</param>
        /// <param name="version">Bot version</param>
        /// <param name="trigger">Bot's trigger</param>
        /// <param name="bdsver">Bot's BDS version</param>
        /// <param name="lastmod">Timestamp of last modification</param>
        public BotInfo(String name, String owner, String type, String version, String trigger, double bdsver, int lastmod)
        {
            Name        = name;
            Owner       = owner;
            Type        = type;
            Version     = version;
            Trigger     = trigger;
            BDSVersion  = bdsver;
            Modified    = lastmod;
        }
    }

    /// <summary>
    /// ClientInfo object. Holds information on a specific client.
    /// </summary>
    public class ClientInfo
    {
        public String Name      = String.Empty;
        public String Type      = String.Empty;
        public String Version   = String.Empty;
        public int Modified     = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Client username</param>
        /// <param name="type">Client type</param>
        /// <param name="version">Client version</param>
        /// <param name="lastmod">Timestamp of last modification</param>
        public ClientInfo(String name, String type, String version, int lastmod)
        {
            Name        = name;
            Type        = type;
            Version     = version;
            Modified    = lastmod;
        }
    }
    #endregion BDS relater

    #region Channel related
    /// <summary>
    /// Privclass object. Stores information on a privclass of a channel.
    /// </summary>
    public class Privclass
    {
        public String Name  = String.Empty;
        public byte Order   = 0;
    }

    /// <summary>
    /// ChatMember object. Stores information on a member of a channel.
    /// </summary>
    public class ChatMember
    {
        public String Name      = String.Empty;
        public String Symbol    = String.Empty;
        public String RealName  = String.Empty;
        public String TypeName  = String.Empty;
        public String GPC       = String.Empty;
        public String Privclass = String.Empty;
    }

    /// <summary>
    /// ChatData object. Stores information on a chatroom.
    /// </summary>
    public class ChatData
    {
        public String Name  = String.Empty;
        public String Title = String.Empty;
        public String Topic = String.Empty;

        public Dictionary<String, Privclass> Privclasses    = new Dictionary<String, Privclass>();
        public Dictionary<String, ChatMember> Members       = new Dictionary<String, ChatMember>();
    }
    #endregion Channel related

    #region strftime

    public enum DayOfWeek : int
    {
        Sunday      = 0,
        Monday      = 1,
        Tuesday     = 2,
        Wednesday   = 3,
        Thursday    = 4,
        Friday      = 5,
        Saturday    = 6
    }

    #endregion strftime

}
