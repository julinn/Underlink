using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UCIS.NaCl;
using UCIS.NaCl.crypto_box;

namespace Underlink
{
    public struct NodeRecord
    {
        public UInt128 Address;
        public byte[] PublicKey;
        public EndPoint Endpoint;
        public int Flags;
    }

    public enum NodeRecordFlags
    {
        None = 0x00,
        Router = 0x02
    }

    public struct NodeKeypair
    {
        public UInt128 Address;
        public byte[] PrivateKey;
        public byte[] PublicKey;
    }

    public struct Node
    {
        public NodeRecord Record;
        public UInt32 LastCommunication;

        public Node(NodeRecord Record)
        {
            this.Record = Record;

            this.LastCommunication = 0;
        }

        public Node(UInt128 Address, IPEndPoint Endpoint, byte[] PublicKey, int Flags)
        {
            this.Record.Address = Address;
            this.Record.Endpoint = Endpoint;
            this.Record.PublicKey = PublicKey;
            this.Record.Flags = Flags;

            this.LastCommunication = 0;
        }

        public override bool Equals(object Obj)
        {
            return this.Record.Address == ((Node)Obj).Record.Address;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Node Left, Node Right)
        {
            if ((object)Left == null || (object)Right == null)
            {
                return (object)Left == (object)Right;
            }

            return Left.Record.Address == Right.Record.Address;
        }

        public static bool operator !=(Node Left, Node Right)
        {
            if ((object)Left == null || (object)Right == null)
            {
                return (object)Left != (object)Right;
            }

            return Left.Record.Address != Right.Record.Address;
        }

        public UInt128 GetDistance(Node CompareNode)
        {
            return GetDistance(CompareNode.Record.Address);
        }

        public UInt128 GetDistance(UInt128 CompareAddress)
        {
            return this.Record.Address.Xor(CompareAddress);
        }
    }

    public class Record
    {
        public static byte[] CreateByteArray(NodeRecord GivenNodeRecord)
        {
            int Length = Marshal.SizeOf(GivenNodeRecord);
            byte[] ReturnByteArray = new byte[Length];

            IntPtr Pointer = Marshal.AllocHGlobal(Length);

            Marshal.StructureToPtr(GivenNodeRecord, Pointer, false);
            Marshal.Copy(Pointer, ReturnByteArray, 0, Length);
            Marshal.FreeHGlobal(Pointer);

            return ReturnByteArray;
        }

        public static NodeRecord CreateNodeRecord(byte[] GivenByteArray)
        {
            NodeRecord ReturnNodeRecord = new NodeRecord();
            int Length = Marshal.SizeOf(ReturnNodeRecord);

            IntPtr Pointer = Marshal.AllocHGlobal(Length);

            Marshal.Copy(GivenByteArray, 0, Pointer, Length);
            ReturnNodeRecord = (NodeRecord)Marshal.PtrToStructure(Pointer, ReturnNodeRecord.GetType());
            Marshal.FreeHGlobal(Pointer);

            return ReturnNodeRecord;
        }
    }
}
