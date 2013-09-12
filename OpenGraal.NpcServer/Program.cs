using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using OpenGraal;
using OpenGraal.Core;
using OpenGraal.NpcServer;
using OpenGraal.Common.Players;

namespace OpenGraal.NpcServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
			Framework framework = new Framework("ncoptions.txt");
			while (framework.RunServer())
				System.Threading.Thread.Sleep(50);
        }
    }
}
