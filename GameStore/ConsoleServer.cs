using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Domain;
using Logic;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;

namespace GameStoreServer
{
    class ConsoleServer
    {
        private static IServiceProvider _serviceProvider;


        static void Main(string[] args)
        {
            Setup setup = new Setup();
            _serviceProvider = setup.BuildServiceProvider();
            Runtime runtime = new Runtime(_serviceProvider);

            setup.InitializeSocketServer(runtime);

        }
    }
}
