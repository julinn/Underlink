using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UCIS.NaCl;
using UCIS.NaCl.crypto_box;

namespace Underlink
{
    class Router
    {
        enum Status
        {
            KEYGEN,
            BOOTSTRAP,
            ISOLATED,
            HEALTHY
        };

        NodeKeypair ThisNodeKeypair;
        Node ThisNode;

        Bucket KnownNodes;

        private Socket Sock;
        private IPEndPoint Endpoint;

        public Router()
        {
            ThisNodeKeypair = GenerateNodeKeypair();
            ThisNode = new Node(ThisNodeKeypair.Address, null);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);

            System.Console.WriteLine("Node ID: " + ThisNodeKeypair.Address.ToHexString());
            System.Console.WriteLine("Private key: " + BitConverter.ToString(ThisNodeKeypair.PrivateKey).Replace("-", ""));
            System.Console.WriteLine("Public key: " + BitConverter.ToString(ThisNodeKeypair.PublicKey).Replace("-", ""));
            KnownNodes.PrintBucketSummary();

            try
            {
                Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Endpoint = new IPEndPoint(IPAddress.Any, 45678);
                Sock.Bind(Endpoint);

                while (true)
                {
                    ArrayList ReadSockets = new ArrayList();
                    EndPoint RemoteEndPoint = null;
                    byte[] ReceiveBuffer = null;

                    ReadSockets.Add(Sock);
                    Socket.Select(ReadSockets, null, null, 1000);

                    if (ReadSockets.Contains(Sock))
                    {
                        // Network socket

                        System.Console.WriteLine("Received information on network socket");

                        Sock.ReceiveFrom(ReceiveBuffer, ref RemoteEndPoint);
                    }
                }
            }
            catch (Exception NetException)
            {
                System.Console.WriteLine("A network exception occured: " + NetException.ToString());
            }
        }

        public NodeKeypair GenerateNodeKeypair()
        {
            NodeKeypair ReturnNodeID = new NodeKeypair();
            ReturnNodeID.PublicKey = new byte[curve25519xsalsa20poly1305.PUBLICKEYBYTES];
            ReturnNodeID.PrivateKey = new byte[curve25519xsalsa20poly1305.SECRETKEYBYTES];
            byte[] AddressBuffer = new byte[32];

            using (RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider())
            {
                using (SHA512 SHAGenerator = new SHA512Managed())
                {
                    while (AddressBuffer[0] != 0xFD)
                    {
                        RandomGenerator.GetBytes(ReturnNodeID.PrivateKey);
                        curve25519xsalsa20poly1305.crypto_box_getpublickey(out ReturnNodeID.PublicKey, ReturnNodeID.PrivateKey);
                        AddressBuffer = SHAGenerator.ComputeHash(ReturnNodeID.PublicKey);
                    }
                }
            }

            ReturnNodeID.Address = new UInt128(AddressBuffer);

            return ReturnNodeID;
        }
    }
}
