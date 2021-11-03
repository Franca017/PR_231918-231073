using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
        
        public async Task<TcpClient> InitializeSocketServerAsync()
        {
            var clientIpEndPoint = new IPEndPoint(IPAddress.Loopback,0);
            var tcpClient = new TcpClient(clientIpEndPoint);
            Console.WriteLine("Trying to connect to server");

            await tcpClient.ConnectAsync(
                IPAddress.Parse(IpConfig),
                Port).ConfigureAwait(false);
            return tcpClient;
        }
    }
}