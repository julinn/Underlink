using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Program
    {
        static void Main(string[] args)
        {
            UInt128 ThisNodeID = new UInt128();
            Node ThisNode = new Node(ThisNodeID);
            Bucket KnownNodes = new Bucket();
        }
    }
}
