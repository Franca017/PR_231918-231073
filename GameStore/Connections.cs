using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace GameStoreServer
{
    public class Connections
    {
        private static bool _exit = false;
        private List<Socket> _clients = new List<Socket>();

        public void ListenConnections(Socket socketServer, IServiceProvider serviceProvider)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    var threadcClient = new Thread(() => StartRuntime(serviceProvider, clientConnected));
                    threadcClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }

            Console.WriteLine("Exiting....");
        }

        private void StartRuntime(IServiceProvider serviceProvider, Socket clientConnected)
        {
            if (!_exit)
            {
                var runtime = new Runtime(serviceProvider);
                runtime.HandleConnection(clientConnected);
                _clients.Remove(clientConnected);
            }
        }

        public void HandleServer(Socket socketServer)
        {
            Console.WriteLine("Bienvenido al Sistema Server");
            while (!_exit)
            {
                Console.WriteLine("Opciones validas: ");
                Console.WriteLine("exit -> abandonar el programa");
                Console.Write("Ingrese su opcion: ");

                var userInput = Console.ReadLine();

                if (userInput.ToLower().Equals("exit"))
                {
                    // Cosas a hacer al cerrar el server
                    // 1 - Cerrar el socket que esta escuchando conexiones nuevas
                    // 2 - Cerrar todas las conexiones abiertas desde los clientes
                    _exit = true;
                    foreach (var client in _clients)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }

                    var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    fakeSocket.Connect("127.0.0.1", 20000);
                    socketServer.Close(0);
                }
                else
                {
                    Console.WriteLine("Opcion incorrecta ingresada");
                }
            }
        }
    }
}