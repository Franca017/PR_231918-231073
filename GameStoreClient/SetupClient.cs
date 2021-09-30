using System.Net;
using System.Net.Sockets;

namespace GameStoreClient
{
    public class Setup
    {
        public Socket InitializeSocketServer()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            socket.Connect("127.0.0.1", 20000);
            return socket;
        }
    }
}