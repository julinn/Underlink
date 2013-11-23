using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class LocalEndpointTunTap : LocalEndpoint
    {
        public LocalEndpointTunTap()
        {

        }

        public byte[] Write(byte[] Buffer)
        {
            return Buffer;
        }

        public byte[] Read()
        {
            return new byte[1];
        }
    }
}
