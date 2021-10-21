using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GameStoreServer
{
    public class Connections
    {
        private bool _exit;
        private readonly List<Socket> _clients = new List<Socket>();

        public async void ListenConnections(TcpListener tcpListener, IServiceProvider serviceProvider)
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
                    var runtime = new Runtime(serviceProvider);
                    runtime.HandleConnection(clientConnected);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing the connection, will not process more data -> Message {e.Message}..");
                    //CloseConnection(clientConnected);
                }
                //_clients.Remove(clientConnected);
            }

            return null; //CHEQUEAR
        }

        private void CloseConnection(Socket client)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
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
                    // Cosas a hacer al cerrar el server
                    // 1 - Cerrar el socket que esta escuchando conexiones nuevas
                    // 2 - Cerrar todas las conexiones abiertas desde los clientes
                    _exit = true;
                    /*foreach (var client in _clients)
                    {
                        CloseConnection(client);
                    }

                    var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    fakeSocket.Connect("127.0.0.1", 20000);
                    socketServer.Close(0);*/
                }
                else
                {
                    Console.WriteLine("Opcion incorrecta ingresada");
                }
            }
        }
    }
}