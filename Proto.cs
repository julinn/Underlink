using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
        public byte[] CreateByteArray(Message GivenMessage)
        {
            int Length = Marshal.SizeOf(GivenMessage);
            byte[] ReturnByteArray = new byte[Length];

            IntPtr Pointer = Marshal.AllocHGlobal(Length);

            Marshal.StructureToPtr(GivenMessage, Pointer, true);
            Marshal.Copy(Pointer, ReturnByteArray, 0, Length);
            Marshal.FreeHGlobal(Pointer);

            return ReturnByteArray;
        }

        public Message CreateMessage(byte[] GivenByteArray)
        {
            Message ReturnMessage = new Message();
            int Length = Marshal.SizeOf(ReturnMessage);

            IntPtr Pointer = Marshal.AllocHGlobal(Length);

            Marshal.Copy(GivenByteArray, 0, Pointer, Length);
            ReturnMessage = (Message) Marshal.PtrToStructure(Pointer, ReturnMessage.GetType());
            Marshal.FreeHGlobal(Pointer);

            return ReturnMessage;
        }
    }
}
