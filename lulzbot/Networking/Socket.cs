using System.Collections.Generic;

/// <summary>
/// This is still in the main namespace, but the sub-namespace is "Networking".
/// So, if we want to use this, we have to add a "using" to the the file we want to 
///  use it in: using lulzbot.Networking
///  Later on, packets will be kept in this namespace as well. (Yes, packets will be done 
///   the OOP way.)
/// </summary>
namespace lulzbot.Networking
{
    /// <summary>
    /// This will be our socket wrapper class.
    /// While, arguably, there's no great reason to really have one, I think it's a good idea.
    /// It makes things a tad cleaner, and also allows for modification of the methods later on,
    ///  without the need for modifying every single usagee. Such is the beauty of OOP design.
    /// </summary>
    
    public class Socket
    {
        // This is private so it can only be accessed within the class.
        // This is a wrapper, not a placeholder. They shouldn't be able to
        //  access this via anything but class methods.
        private Socket _socket;

        // This is our receive buffer. There's no reason for this to be public.
        private byte[] _buffer;

        // In certain cases, we get segmented packets. That's just how TCP works. Now, there are
        //  a few ways to handle this. With dAmn, you can just read until \0 and call it a packet.
        //  But that's not really considered the "correct" way to go about it. It's also inefficient,
        //  when you compare it to just grabbing chunks of data and processing it. So. we basically 
        //  want to use a packet queue instead. We just grab complete packets from the buffer, and add
        //  them to the queue. After which, they're processed from there on. Simple enough?
        private Queue<dAmnPacket> _packet_queue;
    }
}
