using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Node
    {
        public UInt128 Address;

        public Node(UInt128 Address)
        {
            this.Address = Address;
        }

        public UInt128 GetDistance(Node CompareNode)
        {
            return UInt128Functions.Xor(this.Address, CompareNode.Address);
        }
    }
}
