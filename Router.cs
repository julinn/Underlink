using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Underlink
{
    class Router
    {
        UInt128 ThisNodeID;
        Node ThisNode;
        Bucket KnownNodes;
       // LocalEndpoint Endpoint;

        public Router()
        {
            ThisNodeID = new UInt128();
            ThisNode = new Node(ThisNodeID);

            KnownNodes = new Bucket(ThisNode);
            KnownNodes.AddNode(ThisNode);
        }
    }
}
