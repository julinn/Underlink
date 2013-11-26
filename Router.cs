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
            System.Console.WriteLine("Node ID: " + ThisNodeKeypair.Address.ToHexString());
            System.Console.WriteLine("Private key: " + BitConverter.ToString(ThisNodeKeypair.PrivateKey).Replace("-", ""));
            System.Console.WriteLine("Public key: " + BitConverter.ToString(ThisNodeKeypair.PublicKey).Replace("-", ""));

            ThisNode = new Node(ThisNodeKeypair.Address, null);

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
            ReturnNodeID.PublicKey = new byte[curve25519xsalsa20poly1305.PUBLICKEYBYTES];
            ReturnNodeID.PrivateKey = new byte[curve25519xsalsa20poly1305.SECRETKEYBYTES];
            byte[] AddressBuffer = new byte[32];

            RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider();
            SHA512 SHAGenerator = new SHA512Managed();

            while (AddressBuffer[0] != 0xFD)
            {
                RandomGenerator.GetBytes(ReturnNodeID.PrivateKey);
                curve25519xsalsa20poly1305.crypto_box_getpublickey(out ReturnNodeID.PublicKey, ReturnNodeID.PrivateKey);
                AddressBuffer = SHAGenerator.ComputeHash(ReturnNodeID.PublicKey);
            }

            ReturnNodeID.Address = new UInt128(AddressBuffer);

            RandomGenerator.Dispose();
            SHAGenerator.Dispose();

            return ReturnNodeID;
        }
    }
}
