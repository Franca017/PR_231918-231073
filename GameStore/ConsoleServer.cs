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
        private static bool _exit = false;
        private static List<Socket> _clients = new List<Socket>();
        private static IGamesLogic gamesLogic;
        private static IUserLogic userLogic;
        private static IReviewLogic reviewLogic;

        static void Main(string[] args)
        {
            InitializeServices();

            var socketServer = InitializeSocketServer();

            HandleServer(socketServer);
        }

        private static void HandleServer(Socket socketServer)
        {
            Console.WriteLine("Bienvenido al Sistema Server");
            while (!_exit)
            {
                Console.WriteLine("Opciones validas: ");
                Console.WriteLine("ListGames -> abandonar el programa");
                Console.WriteLine("exit -> abandonar el programa");
                Console.WriteLine("Ingrese su opcion: ");

                var userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "ListGames":
                        var games = gamesLogic.GetAll();
                        Console.WriteLine(games.Count);
                        break;
                    // Cosas a hacer al cerrar el server
                    // 1 - Cerrar el socket que esta escuchando conexiones nuevas
                    // 2 - Cerrar todas las conexiones abiertas desde los clientes
                    case "exit":
                        _exit = true;
                        socketServer.Close(0);
                        foreach (var client in _clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }

                        var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        fakeSocket.Connect("127.0.0.1", 20000);
                        break;
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }

        private static Socket InitializeSocketServer()
        {
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);

            var threadServer = new Thread(() => ListenConnections(socketServer));
            threadServer.Start();
            return socketServer;
        }

        private static void InitializeServices()
        {
            IServiceCollection services = new ServiceCollection();
            var startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            gamesLogic = serviceProvider.GetService<IGamesLogic>();
            userLogic = serviceProvider.GetService<IUserLogic>();
            reviewLogic = serviceProvider.GetService<IReviewLogic>();
        }


        private static void ListenConnections(Socket socketServer)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    Console.WriteLine("Accepted new connection...");
                    var threadcClient = new Thread(() => HandleConnection(clientConnected));
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

        private static void HandleConnection(Socket connectedSocket)
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(connectedSocket, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                        case CommandConstants.ListGames:
                            Console.WriteLine("Not Implemented yet...");
                            break;
                        case CommandConstants.Message:
                            Console.WriteLine("Will receive message to display...");
                            var bufferData = new byte[header.IDataLength];  
                            ReceiveData(connectedSocket,header.IDataLength,bufferData);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
        }
        
        private static void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!_exit)
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                        }
                        else
                        {
                            throw new Exception("Server is closing");
                        }
                    }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
            }
        }
    }
}
