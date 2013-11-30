using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using UCIS.NaCl;
using UCIS.NaCl.crypto_box;

namespace Underlink
{
    class Router
    {
        public enum RouterStatus
        {
            KEYGEN,
            BOOTSTRAP,
            ISOLATED,
            HEALTHY
        };

        private Node ThisNode;
        private NodeKeypair ThisNodeKeypair;

        private RouterStatus ThisNodeStatus;
        private Bucket KnownNodes;

        private Socket Sock;
        private IPEndPoint Endpoint;

        public Router()
        {
            ThisNodeStatus = RouterStatus.KEYGEN;

            ThisNodeKeypair = GenerateNodeKeypair();
            ThisNode = new Node(ThisNodeKeypair.Address, null);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);

            Debug.Assert(KnownNodes.Nodes[127, 0] == ThisNode,
                         "The current node must be the first entry in the 128th bucket");

            System.Console.WriteLine("Node ID: " + ThisNodeKeypair.Address.ToHexString());
            System.Console.WriteLine("Private key: " + BitConverter.ToString(ThisNodeKeypair.PrivateKey).Replace("-", ""));
            System.Console.WriteLine("Public key: " + BitConverter.ToString(ThisNodeKeypair.PublicKey).Replace("-", ""));
            KnownNodes.PrintBucketSummary();

            ThisNodeStatus = RouterStatus.BOOTSTRAP;

            try
            {
                Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Endpoint = new IPEndPoint(IPAddress.Any, 45678);
                Sock.Bind(Endpoint);

                while (true)
                {
                    ArrayList ReadSockets = new ArrayList();
                    EndPoint RemoteEndPoint = null;
                    EndPoint SendEndPoint = null;

                    byte[] ReceiveBuffer = null;
                    byte[] SendBuffer = null;
                    Message ReceiveMessage;
                    Message SendMessage = new Message();

                    ReadSockets.Add(Sock);
                    Socket.Select(ReadSockets, null, null, 1000);

                    if (ReadSockets.Contains(Sock))
                    {
                        Sock.ReceiveFrom(ReceiveBuffer, ref RemoteEndPoint);
                        ReceiveMessage = ProtoMarshal.CreateMessage(ReceiveBuffer);

                        System.Console.WriteLine("Received " + ReceiveMessage.Type.ToString() + " message on socket");

                        switch (ReceiveMessage.Type)
                        {
                            case MessageType.Verify:
                                SendMessage.Type = MessageType.VerifySuccess;
                                SendMessage.LocalID = ThisNode.Record.Address;
                                SendMessage.RemoteID = ReceiveMessage.LocalID;
                                SendMessage.TTL = 24;
                                SendMessage.Flags = 0;
                                SendMessage.Payload = Record.CreateByteArray(ThisNode.Record);
                                SendMessage.PayloadSize = SendMessage.Payload.Length;
                                break;

                            case MessageType.VerifySuccess:
                                Node ReceivedNode = new Node(Record.CreateNodeRecord(ReceiveMessage.Payload));
                                KnownNodes.AddNode(ReceivedNode);
                                break;
                        }

                        if (SendMessage.RemoteID != null)
                        {
                            SendBuffer = ProtoMarshal.CreateByteArray(SendMessage);
                            SendEndPoint = KnownNodes.GetClosestNode(SendMessage.RemoteID, 0).Record.Endpoint;

                            Sock.SendTo(SendBuffer, SendEndPoint);
                        }
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

        public RouterStatus GetRouterStatus()
        {
            return ThisNodeStatus;
        }
    }
}
