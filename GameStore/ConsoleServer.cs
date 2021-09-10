using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameStoreServer
{
    class ConsoleServer
    {
        static bool _exit = false;

        static void Main(string[] args)
        {
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);

            while (!_exit)
            {
                var connectedSocket = socketServer.Accept();

                Console.WriteLine("Accepted a connection");

                var thread = new Thread(() => HandleConnection(connectedSocket));
                thread.Start();
            }

        }

        private static void HandleConnection(Socket connectedSocket)
        {
            var bytesReceived = 1;
            while (bytesReceived > 0)
            {
                var buffer = new byte[1024];
                bytesReceived = connectedSocket.Receive(buffer);
                if (bytesReceived > 0)
                {
                    var message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine(message);
                }
            }

            connectedSocket.Shutdown(SocketShutdown.Both);
            connectedSocket.Close();

            Console.WriteLine("Closed connection");
        }
    }
}
