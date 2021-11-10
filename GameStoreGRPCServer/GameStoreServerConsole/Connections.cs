using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameStoreGRPCServer.GameStoreServerConsole
{
    public class Connections
    {
        private bool _exit;

        public async Task ListenConnectionsAsync(TcpListener tcpListener, IServiceProvider serviceProvider)
        {
            while (!_exit)
            {
                try
                {
                    var tcpClientSocket = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var task = Task.Run(async () => await StartRuntime(serviceProvider, tcpClientSocket).ConfigureAwait(false));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }

            Console.WriteLine("Exiting....");
        }

        private Task StartRuntime(IServiceProvider serviceProvider, TcpClient clientConnected)
        {
            if (!_exit)
            {
                try
                {
                    var runtime = new Runtime(serviceProvider,clientConnected);
                    runtime.HandleConnectionAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing the connection, will not process more data -> Message {e.Message}..");
                }
            }

            return null;
        }

        public void HandleServer()
        {
            Console.WriteLine("Bienvenido al Sistema Server");
            while (!_exit)
            {
                Console.WriteLine("Opciones validas: ");
                Console.WriteLine("exit -> abandonar el programa");
                Console.Write("Ingrese su opcion: ");

                var userInput = Console.ReadLine();

                if (userInput != null && userInput.ToLower().Equals("exit"))
                {
                    _exit = true;
                }
                else
                {
                    Console.WriteLine("Opcion incorrecta ingresada");
                }
            }
        }
    }
}