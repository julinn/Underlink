using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    abstract class LocalEndpoint
    {
        public abstract byte[] Write(byte[] Buffer);
        public abstract byte[] Read();
    }
}
