using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Domain;
using Logic;
using ProtocolLibrary;

namespace GameStoreClient
{
    public class Runtime
    {
        private bool _exit = false;
        private List<Game> gamesLoaded = new List<Game>();
        private int userId;

        public void Execute(Socket socket)
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.Write("Ingrese su nombre de Usuario (en caso de no existir se le creara uno): ");
            var user = Console.ReadLine();
            Request(user, socket, CommandConstants.Login);
            var bufferResponse = Response(socket);
            
            Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
            //Setear el userId

            while (!_exit)
            {
                Console.WriteLine("Opciones validas: ");
                Console.WriteLine("list -> Visualiza la lista de juegos");
                Console.WriteLine("message -> envia un mensaje al server");
                Console.WriteLine("exit -> abandonar el programa");
                Console.Write("Ingrese su opcion: ");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "list":
                        ListGames(socket);
                        break;
                    case "exit":
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        _exit = true;
                        break;
                    case "message":
                        Console.WriteLine("Ingrese el mensaje a enviar:");
                        var message = Console.ReadLine();
                        Request(message, socket, CommandConstants.Message);

                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }

        private void ListGames(Socket socket)
        {
            Request("", socket, CommandConstants.ListGames);
            var bufferResponse = Response(socket);
            var length = Convert.ToInt32(Encoding.UTF8.GetString(bufferResponse));
            gamesLoaded.Clear();
            Console.WriteLine("\n Games list: \n");
            for (int i = 0; i < length; i++)
            {
                bufferResponse = Response(socket);
                var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                var game = new Game(split[1], split[2], split[4]);
                game.Id = Convert.ToInt32(split[0]);
                game.Rating = Convert.ToInt32(split[3]);
                game.Image = split[5];
                gamesLoaded.Add(game);
                Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
            }
            
            var main = false;
            while (!main)
            {
                Console.WriteLine("\n Options:");
                Console.WriteLine("detail -> Get details of a game");
                Console.WriteLine("purchase -> Purchase a game");
                Console.WriteLine("main <- Go to main menu");
                Console.Write("Option: ");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "detail":
                        DetailGame();
                        break;
                    case "purchase":
                        Purchase(socket);
                        break;
                    case "main":
                        main = true;
                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }
        }

        private void Purchase(Socket socket)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to purchase: ");
                var gameId = Console.ReadLine();
                var game = gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Request(gameId, socket, CommandConstants.Purchase);
                    var bufferResponse = Response(socket);
            
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    idCorrecto = true;
                }
            }
        }

        private void DetailGame()
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to select: ");
                var id = Console.ReadLine();
                var game = gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(id)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Console.WriteLine($" --- {game.Title} --- ");
                    Console.Write($"{game.Genre} *{game.Rating}");
                    for (int i = 0; i < game.Rating; i++)
                    {
                        Console.Write("*");
                    }
                    Console.WriteLine("\n"+game.Sinopsis);
                    idCorrecto = true;
                }
            }
        }

        private byte[] Response(Socket socket)
        {
            byte[] bufferResponse = new byte[] { };
            var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                               HeaderConstants.DataLength;
            var buffer = new byte[headerLength];
            try
            {
                ReceiveData(socket, headerLength, buffer);
                var header = new Header();
                header.DecodeData(buffer);
                bufferResponse = new byte[header.IDataLength];
                ReceiveData(socket, header.IDataLength, bufferResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine($"---- -> Message {e.Message}..");
            }

            return bufferResponse;
        }

        private void Request(string mensaje, Socket socket, int command)
        {
            var header = new Header(HeaderConstants.Request, command, mensaje.Length);
            var data = header.GetRequest();
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }

            sentBytes = 0;
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }
        
        private void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
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