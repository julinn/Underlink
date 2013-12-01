using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Underlink
{
    class Program
    {
        static void Main(string[] args)
        {
            Router RouterInstance = new Router();

            /*
            Thread FirstThread = new Thread(() => { Router RouterInstance = new Router(); });
            FirstThread.Start();

            Thread SecondThread = new Thread(() => { Router RouterInstance = new Router(); });
            SecondThread.Start();
             */

            Console.ReadLine();
        }
    }
}
