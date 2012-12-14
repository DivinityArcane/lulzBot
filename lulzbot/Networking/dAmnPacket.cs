using System;
using System.Collections.Generic;

namespace lulzbot.Networking
{
    /// <summary>
    /// This will be our dAmnPacket class. It will basically be an object that we
    ///  pass data to, and it will process the data and create a viable packet object
    ///  from it. From there on, we can use the object to handle events and respond.
    /// </summary>
    public class dAmnPacket
    {
        // Here are our public properties, which will hold our important data!
        
        // This shouldn't ever change in dAmn, but who knows. It's the separator between
        //  argument names and their data.
        // This is flagged "const" (a constant) so that it cannot be changed. It's also
        //  static by default, and is easily optimized by the compiler.
        private const String Separator = "=";

        // I prefer to initialize strings with the String.Empty value (""). This seems
        //  neater to me, and avoids errors if you somehow add to/use them before assignment.
        public String Command       = String.Empty;
        public String Parameter     = String.Empty;
        public String SubCommand    = String.Empty;
        public String SubParameter  = String.Empty;
        public String Body          = String.Empty;
        public String Raw           = String.Empty;

        // For the arguments, we'll use a dictionary. While, in the past, I used different types
        //  to handle duplicate keys, I'm not going to. Why? dAmn should _not_ send duplicate argument
        //  names.
        public Dictionary<String, String> Arguments = new Dictionary<String, String>();

        /// <summary>
        /// Here, we'll parse the string data of a packet and create our object.
        /// </summary>
        /// <param name="data">The string data of a packet.</param>
        public void Parse(String data)
        {

        }
    }
}
