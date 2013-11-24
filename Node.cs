using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    struct Node
    {
        public UInt128 Address;
        public IPEndPoint Endpoint;

        public Node(UInt128 Address, IPEndPoint Endpoint)
        {
            this.Address = Address;
            this.Endpoint = Endpoint;
        }

        public override bool Equals(object Obj)
        {
            return this.Address == ((Node) Obj).Address;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Node Left, Node Right)
        {
            if ((object) Left == null || (object) Right == null)
            {
                return (object) Left == (object) Right;
            }

            return Left.Address == Right.Address;
        }

        public static bool operator !=(Node Left, Node Right)
        {
            if ((object)Left == null || (object)Right == null)
            {
                return (object)Left != (object)Right;
            }

            return Left.Address != Right.Address;
        }

        public UInt128 GetDistance(Node CompareNode)
        {
            return this.Address.Xor(CompareNode.Address);
        }
    }
}
