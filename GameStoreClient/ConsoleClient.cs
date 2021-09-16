using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolLibrary;

namespace GameStoreClient
{
    class ConsoleClient
    {
        static void Main(string[] args)
        {
            Setup setup = new Setup();
            Runtime runtime = new Runtime();

            var socket = setup.InitializeSocketServer(runtime);
            
            runtime.Execute(socket);
        }
    }
}
