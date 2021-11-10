using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GameStoreGRPCServer.GameStoreServerConsole
{
    public class Setup : BackgroundService
    {
        private string IpConfig { get; set; }
        private int Port { get; set; }
        private readonly IServiceProvider _serviceProvider;

        public Setup(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ipEndPoint = new IPEndPoint(
                IPAddress.Parse(IpConfig),
                Port);
            var tcpListener = new TcpListener(ipEndPoint);
            tcpListener.Start(100);
            var connections = new Connections();

            var task = Task.Run(async () => await connections.ListenConnectionsAsync(tcpListener, _serviceProvider), stoppingToken).ConfigureAwait(false);

            Console.WriteLine($"IpConfig: {IpConfig} - Port: {Port}");
            connections.HandleServer();

            return null;
        }
    }
}