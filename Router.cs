using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Router
    {
        UInt128 ThisNodeID;
        Node ThisNode;

        Bucket KnownNodes;

        LocalEndpoint Adapter;
        NetworkEndpoint Socket;

        public Router()
        {
            ThisNodeID = GenerateNodeID();
            System.Console.WriteLine("This node ID: " + ThisNodeID.ToHexString());
            
            ThisNode = new Node(ThisNodeID, null);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);

            Adapter = new LocalEndpointTunTap();
            Socket = new NetworkEndpointUDP(3090);

            for (int i = 0; i < 323; i ++)
            {
                Node TestNode = new Node(GenerateNodeID(), null);
                KnownNodes.AddNode(TestNode);
            }
        }

        public UInt128 GenerateNodeID()
        {
            UInt128 ReturnNodeID;

            byte[] BigBuffer = new byte[sizeof(Int64)];
            byte[] SmallBuffer = new byte[sizeof(Int64)];

            Random RandomGenerator = new Random();

            RandomGenerator.NextBytes(BigBuffer);
            RandomGenerator.NextBytes(SmallBuffer);
            BigBuffer[0] = 0xFD;

            ReturnNodeID.Big = BitConverter.ToUInt64(BigBuffer, 0);
            ReturnNodeID.Small = BitConverter.ToUInt64(SmallBuffer, 0);

            return ReturnNodeID;
        }
    }
}
