using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    public enum MessageType
    {
        IPPacket,
        ForwardedRefer,
        ForwardedNoRefer,
        NotForwarded,
        Verify,
        VerifySuccess,
        UnspecError
    }

    public struct Message
    {
        MessageType Type;
        int PayloadSize;
        UInt128 LocalID;
        UInt128 RemoteID;
        int TTL;
        int Flags;


    }

    class Proto
    {
        
    }
}
