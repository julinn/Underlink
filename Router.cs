using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        public Router()
        {
            ThisNodeStatus = RouterStatus.KEYGEN;

            System.Console.WriteLine("Generating node ID and keypair...");
            ThisNodeKeypair = GenerateNodeKeypair();
            ThisNode = new Node(ThisNodeKeypair.Address, new IPEndPoint(IPAddress.Loopback, 45678), ThisNodeKeypair.PublicKey);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);

            Debug.Assert(KnownNodes.Nodes[127, 0] == ThisNode,
                         "The current node must be the first entry in the 128th bucket");

            System.Console.WriteLine("Node ID: " + ThisNodeKeypair.Address.ToHexString());
            System.Console.WriteLine("Private key: " + BitConverter.ToString(ThisNodeKeypair.PrivateKey).Replace("-", ""));
            System.Console.WriteLine("Public key: " + BitConverter.ToString(ThisNodeKeypair.PublicKey).Replace("-", ""));
            KnownNodes.PrintBucketSummary();

            ThisNodeStatus = RouterStatus.BOOTSTRAP;

            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Sock.Bind(ThisNode.Record.Endpoint);

            Thread SocketThread = new Thread(() =>
            {
                while (true)
                {
                    ArrayList ReadSockets = new ArrayList();
                    EndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint SendEndPoint = null;

                    byte[] ReceiveBuffer = new byte[1500];
                    byte[] SendBuffer = null;

                    ReadSockets.Add(Sock);
                    Socket.Select(ReadSockets, null, null, 10000000);

                    if (ReadSockets.Contains(Sock))
                    {
                        Sock.ReceiveFrom(ReceiveBuffer, ref RemoteEndPoint);
                        Message ReceiveMessage = ProtoMarshal.CreateMessage(ReceiveBuffer);
                        Message SendMessage = SendMessage = ProcessMessage(ReceiveMessage);

                        if (SendMessage.LocalID != null &&
                            SendMessage.RemoteID != null &
                            SendMessage.Type != MessageType.IPPacket)
                        {
                            SendBuffer = ProtoMarshal.CreateByteArray(SendMessage);
                            SendEndPoint = KnownNodes.GetClosestNode(SendMessage.RemoteID, 0).Record.Endpoint;

                            Sock.SendTo(SendBuffer, SendEndPoint);
                        }
                    }
                        else
                    {
                        Message VerifyTest = new Message();
                        VerifyTest.LocalID = ThisNode.Record.Address;
                        VerifyTest.RemoteID = ThisNode.Record.Address;
                        VerifyTest.Type = MessageType.Verify;
                        VerifyTest.Flags = 0;
                        VerifyTest.Payload = Record.CreateByteArray(ThisNode.Record);
                        VerifyTest.PayloadSize = VerifyTest.Payload.Length;

                        byte[] VerifyTestBuffer = ProtoMarshal.CreateByteArray(VerifyTest);
                        Node VerifyTestNode = KnownNodes.GetClosestNode(VerifyTest.RemoteID, 0);
                        EndPoint VerifyTestEndPoint = VerifyTestNode.Record.Endpoint;

                        Sock.SendTo(VerifyTestBuffer, VerifyTestEndPoint);
                    }
                }
            });

            Thread TUNTAPThread = new Thread(() =>
            {
                IList<NetworkAdapterWin> Adapters = NetworkAdapterWin.GetAdapters();
                LocalEndpoint Adapter = Adapters[0].Open();

                byte[] ReceiveBuffer = new byte[1500];

                while (true)
                {
                    if (!Adapter.CanRead)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    Adapter.Read(ReceiveBuffer, 0, 1500);
                    System.Console.WriteLine("Buffer bytes: " + ReceiveBuffer.Length);
                }

                System.Console.WriteLine(Adapters.ToString());
            });

            SocketThread.Start();
            TUNTAPThread.Start();
        }

        public Message ProcessMessage(Message ReceiveMessage)
        {
            Message SendMessage = new Message();
            ProtoMarshal.PrintMessage(ReceiveMessage);

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

            return SendMessage;
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
