using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;

namespace GameStoreServer
{
    public class Setup
    {
        public IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
        
        public void InitializeSocketServer(Runtime runtime)
        {
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);
            Connections connections = new Connections();

            var threadServer = new Thread(() => connections.ListenConnections(socketServer, runtime));
            threadServer.Start();
            
            connections.HandleServer(socketServer);
        }
    }
}