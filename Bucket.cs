using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Bucket
    {
        public const int NodeAddressLength = 128;
        public const int NodesPerBucket = 16;

        public Node[,] Nodes = new Node[NodeAddressLength, NodesPerBucket];
        private Node ThisNode;

        public Bucket(Node ThisNode)
        {
            this.ThisNode = ThisNode;
        }

        public int GetBucketID(Node CheckNode)
        {
            return GetBucketID(CheckNode.Record.Address);
        }

        public int GetBucketID(UInt128 CheckAddress)
        {
            UInt128 Bitmask = new UInt128(UInt64.MaxValue, UInt64.MaxValue);

            for (int i = 0; i < NodeAddressLength; i ++)
            {
                if (i < 64)
                    Bitmask.Small <<= 1;
                else
                    Bitmask.Big <<= 1;

                if (ThisNode.Record.Address.MaskEquals(CheckAddress, Bitmask))
                    return NodeAddressLength - i - 1;
            }

            return NodeAddressLength - 1;
        }

        public int AddNode(Node NewNode)
        {
            int BucketID = GetBucketID(NewNode);

            // First see if we already know about this node,
            // and simply update it with the new data if we do

            for (int n = 0; n < NodesPerBucket; n ++)
            {
                if (Nodes[BucketID, n].Record.Address == NewNode.Record.Address)
                {
                    Nodes[BucketID, n] = NewNode;
                    return BucketID;
                }
            } 

            // Then scan for empty slots in the bucket

            for (int n = 0; n < NodesPerBucket; n ++)
            {
                if (Nodes[BucketID, n].Record.Address.IsZero())
                {
                    // System.Console.WriteLine("Bucket " + (BucketID + 1) + ": Added new node " + n);

                    Nodes[BucketID, n] = NewNode;
                    return BucketID;
                }
            }

            // Finally, if there's no empty slots, overwrite the
            // most distant node with the new one

            int MostDistant = 0;

            for (int n = 0; n < NodesPerBucket; n++)
            {
                // UInt128 Distance = Nodes[BucketID, n].GetDistance(NewNode);
                UInt128 Distance = ThisNode.GetDistance(NewNode);

                if (Distance > Nodes[BucketID, MostDistant].Record.Address)
                    MostDistant = n;
            }

            Nodes[BucketID, MostDistant] = NewNode;

           // System.Console.WriteLine("Bucket " + (BucketID + 1) + ": Replaced existing node " + MostDistant);
            return BucketID;
        }

        public void DeleteNode(Node DeleteNode)
        {
            return;
        }

        public Node GetClosestNode(Node SearchNode, int Steps)
        {
            return GetClosestNode(SearchNode.Record.Address, Steps);
        }

        public Node GetClosestNode(UInt128 SearchAddress, int Steps)
        {
            int StartBucket = GetBucketID(SearchAddress);
            if (StartBucket == 0 || SearchAddress == ThisNode.Record.Address)
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

                    if (SortedNodes[n].Record.Address == SearchAddress)
                        return SortedNodes[n];

                    if (SortedNodes[n].GetDistance(SearchAddress) < LastDistance)
                    {
                        ReturnNode = SortedNodes[n];
                        LastDistance = SortedNodes[n].GetDistance(SearchAddress);
                    }
                }
            }

            return ReturnNode;
        }

        public void PrintBucketSummary()
        {
            for (int b = 0; b < NodeAddressLength; b++)
            {
                int count = 0;

                for (int n = 0; n < NodesPerBucket; n++)
                {
                    if (!Nodes[b, n].Record.Address.IsZero())
                        count++;
                }

                if (count == NodesPerBucket)
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                else if (count > 0)
                    System.Console.ForegroundColor = ConsoleColor.Green;
                else
                    System.Console.ForegroundColor = ConsoleColor.Red;

                System.Console.Write((b + 1).ToString("D3") + ": " + count + "\t");
                if (b % 8 == 7)
                    System.Console.WriteLine();
            }

            System.Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
