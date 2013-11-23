using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Bucket
    {
        private const int NodeAddressLength = 128;
        private const int NodesPerBucket = 16;

        private Node[,] Nodes = new Node[NodeAddressLength, NodesPerBucket];

        public void AddNode(Node NewNode)
        {
            return;
        }

        public void DeleteNode(Node DeleteNode)
        {
            return;
        }

        public Node GetClosestNode(Node SearchNode)
        {
            return SearchNode;
        }
    }
}
