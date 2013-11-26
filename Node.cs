using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UCIS.NaCl;
using UCIS.NaCl.crypto_box;

namespace Underlink
{
    public struct NodeRecord
    {
        public UInt128 Address;
        public IPEndPoint Endpoint;
    }

    public struct NodeKeypair
    {
        public UInt128 PrivateKey;
        public UInt128 PublicKey;
    }

    struct Node
    {
        public NodeRecord Record;
        public UInt32 LastCommunication;

        public Node(UInt128 Address, IPEndPoint Endpoint)
        {
            this.Record.Address = Address;
            this.Record.Endpoint = Endpoint;

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
            return this.Record.Address.Xor(CompareNode.Record.Address);
        }
    }
}
