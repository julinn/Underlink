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
        private Node ThisNode;

        public Bucket(Node ThisNode)
        {
            this.ThisNode = ThisNode;
        }

        public int GetBucketID(Node CheckNode)
        {
            UInt128 Bitmask = new UInt128(0, 0);

            for (int i = 0; i < NodeAddressLength; i ++)
            {
                if (i < 64)
                    Bitmask.Small <<= 1;
                else
                    Bitmask.Big <<= 1;

                System.Console.WriteLine(i + " -> " + Convert.ToString((long) Bitmask.Big, 2).PadLeft(8, '0'));
                System.Console.WriteLine(i + " -> " + Convert.ToString((long) Bitmask.Small, 2).PadLeft(8, '0'));

              //  if (ThisNode.Address.MaskEquals(Bitmask, CheckNode.Address, Bitmask))
              //      return NodeAddressLength - 1 - i;
            }

            return NodeAddressLength - 1;
        }

        public int AddNode(Node NewNode)
        {
            int BucketID = GetBucketID(NewNode);

            System.Console.WriteLine("In bucket " + BucketID);

            // First scan for the current node

            for (int n = 0; n < NodesPerBucket; n ++)
            {
                if (ThisNode == NewNode)
                    return BucketID;
            }

            // Then scan for empty slots in the bucket

            for (int n = 0; n < NodesPerBucket; n ++)
            {
                System.Console.WriteLine("Bucket " + BucketID + " node " + n);

                if (Nodes[BucketID, n] == null)
                {
                    Nodes[BucketID, n] = NewNode;

                    System.Console.WriteLine("Added new node");
                    return BucketID;
                }
            }

            // Finally, if there's no empty slots, overwrite the
            // most distant node with the new one

            int MostDistant = 0;

            for (int n = 0; n < NodesPerBucket; n++)
            {
                UInt128 Distance = Nodes[BucketID, n].GetDistance(NewNode);

                if (Distance > Nodes[BucketID, MostDistant].Address)
                    MostDistant = n;
            }

            Nodes[BucketID, MostDistant] = NewNode;

            System.Console.WriteLine("Replaced node " + MostDistant);
            return BucketID;
        }

        public void DeleteNode(Node DeleteNode)
        {
            return;
        }

        public Node GetClosestNode(Node SearchNode, int Steps)
        {
            int StartBucket = GetBucketID(SearchNode);
            if (StartBucket == 0 || SearchNode == ThisNode)
                return ThisNode;

            UInt128 LastDistance = new UInt128(0, 0);
            Node ReturnNode = new Node();

            for (int b = 0; b < NodeAddressLength; b ++)
            {
                Node[] SortedNodes = new Node[NodesPerBucket];
                for (int k = 0; k < NodesPerBucket; k ++)
                    SortedNodes[k] = Nodes[StartBucket, k];

                Array.Sort(SortedNodes,
                    delegate(Node A, Node B)
                    {
                        UInt128 DistFromA = ThisNode.GetDistance(A);
                        UInt128 DistFromB = ThisNode.GetDistance(B);

                        if (DistFromA < DistFromB) return 1;
                        if (DistFromA > DistFromB) return -1;
                        return 0;
                    });

                for (int n = Steps; n < NodesPerBucket; n ++)
                {
                    if (SortedNodes[n] == null)
                        continue;

                    if (SortedNodes[n] == SearchNode)
                        return SortedNodes[n];

                    if (SortedNodes[n].GetDistance(SearchNode) < LastDistance)
                    {
                        ReturnNode = SortedNodes[n];
                        LastDistance = SortedNodes[n].GetDistance(SearchNode);
                    }
                }
            }

            return SearchNode;
        }
    }
}
