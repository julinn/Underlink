using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class NetworkEndpointUDP : NetworkEndpoint
    {
        private Socket Sock;
        private IPEndPoint Endpoint;

        private byte[] ReceiveBuffer;
        private EndPoint Sender = new IPEndPoint(IPAddress.Any, 0); 

        public NetworkEndpointUDP(int PortNumber)
        {
            try
            {
                Sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Endpoint = new IPEndPoint(IPAddress.Any, PortNumber);

                Sock.Bind(Endpoint);

            //    Sock.BeginReceiveFrom(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, ref Sender, new AsyncCallback(onReceive), Sender);
            }
            catch (Exception NetException)
            {
                System.Console.WriteLine("A network exception occured: " + NetException.ToString());
            }
        }

        public override byte[] Write(byte[] Buffer)
        {
            return Buffer;
        }

        public override byte[] Read()
        {
            return new byte[1];
        }
    }
}
