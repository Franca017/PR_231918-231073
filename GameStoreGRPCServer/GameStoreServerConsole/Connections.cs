using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameStoreGRPCServer.GameStoreServerConsole
{
    public class Connections
    {
        public async Task ListenConnectionsAsync(TcpListener tcpListener, IServiceProvider serviceProvider)
        {
            while (!Exit.Instance)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    if (Exit.Instance)
                    {
                        tcpListener.Stop();
                    }
                    else
                    {
                        var task = Task.Run(async () =>
                            await StartRuntime(serviceProvider, tcpClientSocket).ConfigureAwait(false));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Exit.Instance = true;
                }
            }

            Console.WriteLine("Closing clients server...");
        }

        private async Task StartRuntime(IServiceProvider serviceProvider, TcpClient clientConnected)
        {
            try
            {
                var runtime = new Runtime(serviceProvider,clientConnected);
                await runtime.HandleConnectionAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Server is closing the connection, will not process more data -> Message {e.Message}..");
            }
        }

        
    }
}