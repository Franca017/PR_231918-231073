using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ProtocolLibrary;

namespace GameStoreClient
{
    public class Setup
    {
        private string IpConfig { get; set; }
        private int Port { get; set; }
        static readonly ISettingsManager SettingsMgr = new SettingsManager();

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
        
        public async Task<TcpClient> InitializeSocketServer()
        {
            var clientIpEndPoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.7"),
                45000);
            var tcpClient = new TcpClient(clientIpEndPoint);
            Console.WriteLine("Trying to connect to server");

            await tcpClient.ConnectAsync(
                IPAddress.Parse(IpConfig),
                Port).ConfigureAwait(false);
            return tcpClient;
        }
    }
}