using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UCIS.NaCl;
using UCIS.NaCl.crypto_box;

namespace Underlink
{
    class Router
    {
        NodeKeypair ThisNodeKeypair;
        Node ThisNode;

        Bucket KnownNodes;

        LocalEndpoint Adapter;
        NetworkEndpoint Socket;

        public Router()
        {
            ThisNodeKeypair = GenerateNodeKeypair();
            System.Console.WriteLine("This node ID: " + ThisNodeKeypair.PublicKey.ToHexString());
            System.Console.WriteLine("Private key: " + ThisNodeKeypair.PrivateKey.ToHexString());

            ThisNode = new Node(ThisNodeKeypair.PublicKey, null);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);

           // Adapter = new LocalEndpointTunTap();
            Socket = new NetworkEndpointUDP(3090);

           /* for (int i = 0; i < 2000; i ++)
            {
                Node TestNode = new Node(GenerateNodeID(), null);
                KnownNodes.AddNode(TestNode);
            } */

            KnownNodes.PrintBucketSummary();
        }

        public NodeKeypair GenerateNodeKeypair()
        {
            NodeKeypair ReturnNodeID = new NodeKeypair();
            byte[] PublicKeyBuffer = new byte[curve25519xsalsa20poly1305.PUBLICKEYBYTES];
            byte[] PrivateKeyBuffer = new byte[curve25519xsalsa20poly1305.SECRETKEYBYTES];

            Random RandomGenerator = new Random(Guid.NewGuid().GetHashCode());
            SHA512 SHAGenerator = new SHA512Managed();

            while (PublicKeyBuffer[0] != 0xFD)
            {
                RandomGenerator.NextBytes(PrivateKeyBuffer);
                PublicKeyBuffer = SHAGenerator.ComputeHash(PrivateKeyBuffer);

                curve25519xsalsa20poly1305.crypto_box_getpublickey(out PublicKeyBuffer, PrivateKeyBuffer);
            }

            ReturnNodeID.PublicKey = new UInt128(PublicKeyBuffer);
            ReturnNodeID.PrivateKey = new UInt128(PrivateKeyBuffer);

            return ReturnNodeID;
        }
    }
}
