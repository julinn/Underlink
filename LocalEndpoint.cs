using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    public abstract class LocalEndpoint
    {
        public abstract void Write(byte[] buffer, int offset, int count);
        public abstract int Read(byte[] buffer, int offset, int count);
        public abstract void Close();

        public abstract bool CanRead { get; }
        public abstract bool CanSeek { get; }
        public abstract bool CanTimeout { get; }
        public abstract bool CanWrite { get; }

        public abstract int ReadTimeout { get; set; }
        public abstract int WriteTimeout { get; set; }
    }
}
