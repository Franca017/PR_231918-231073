using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;

namespace GameStoreServer
{
    public class Setup
    {
        static readonly ISettingsManager SettingsManager = new SettingsManager();
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
                Port = Int32.Parse(configuration["Connection:PORT"]);
            }
            catch (Exception)
            {
                Console.WriteLine("Configuration file missing");
                Environment.Exit(0);
            }
        }
        
        public IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
        
        public void InitializeSocketServer(IServiceProvider serviceProvider)
        {
            Console.WriteLine(SettingsManager.ReadSetting(ServerConfig.ServerIpConfigKey));
            var ipEndPoint = new IPEndPoint(
                IPAddress.Parse(IpConfig),
                Port);
            var tcpListener = new TcpListener(ipEndPoint);
            tcpListener.Start(100);
            var connections = new Connections();

            var threadServer = new Thread(() => connections.ListenConnections(tcpListener, serviceProvider));
            threadServer.Start();
            
            Console.WriteLine($"IpConfig: {IpConfig} - Port: {Port}");
            connections.HandleServer();
        }
    }
}