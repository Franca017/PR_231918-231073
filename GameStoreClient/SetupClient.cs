using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace GameStoreClient
{
    public class Setup
    {
        private string IpConfig { get; set; }
        private int Port { get; set; }

        public Setup()
        {
            ReadAppSettings();
        }

        private void ReadAppSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile("appsettings.json", false, true);
                var configuration = builder.Build();
                IpConfig = configuration["Connection:IP"];
                Port = int.Parse(configuration["Connection:PORT"]);
            }
            catch (Exception)
            {
                Console.WriteLine("Configuration file missing");
                Environment.Exit(0);
            }
        }
        
        public Socket InitializeSocketServer()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(IpConfig), 0));
            socket.Connect(IpConfig, Port);
            return socket;
        }
    }
}