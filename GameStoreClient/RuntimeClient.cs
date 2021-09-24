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

        public void Execute(Socket socket)
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.Write("Ingrese su nombre de Usuario (en caso de no existir se le creara uno): ");
            var user = Console.ReadLine();
            Request(user, socket, CommandConstants.Login);
            var bufferResponse = Response(socket, CommandConstants.Login);
            
            Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));

            while (!_exit)
            {
                Console.WriteLine("\n Options: ");
                Console.WriteLine("list -> Visualiza la lista de juegos");
                Console.WriteLine("publish -> Publicar un juego");
                Console.WriteLine("publishedgames -> Visualiza la lista de juegos publicados");
                Console.WriteLine("message -> envia un mensaje al server");
                Console.WriteLine("exit -> abandonar el programa");
                Console.Write("Ingrese su opcion: ");
                var option = Console.ReadLine();
                if (option != null)
                    switch (option.ToLower())
                    {
                        case "list":
                            ListGames(socket);
                            break;
                        case "publish":
                            Publish(socket);
                            break;
                        case "publishedgames":
                            ListPublishedGames(socket);
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

        private void ListPublishedGames(Socket socket)
        {
            Request("", socket, CommandConstants.ListPublishedGames);
            var bufferResponse = Response(socket, CommandConstants.ListPublishedGames);
            var lengthString = Encoding.UTF8.GetString(bufferResponse);
            var length = Convert.ToInt32(lengthString);
            var gamesPublished = new List<Game>();
            Console.WriteLine("\n Published games list: \n");
            for (int i = 0; i < length; i++)
            {
                bufferResponse = Response(socket, CommandConstants.ListPublishedGames);
                var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                var game = new Game(split[1], split[2], split[4]);
                game.Id = Convert.ToInt32(split[0]);
                game.Rating = Convert.ToInt32(split[3]);
                game.Image = split[5];
                gamesPublished.Add(game);
                Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
            }
            
            var main = false;
            while (!main)
            {
                Console.WriteLine("\n Options:");
                Console.WriteLine("delete -> Get details of a game");
                Console.WriteLine("main <- Go to main menu");
                Console.Write("Option: ");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "delete":
                        DeleteGame(socket, gamesPublished);
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

        private void DeleteGame(Socket socket, List<Game> gamesPublished)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to delete: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Request(gameId, socket, CommandConstants.DeleteGame);
                    var bufferResponse = Response(socket, CommandConstants.DeleteGame);
            
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    idCorrecto = true;
                }
            }
        }

        private void ListGames(Socket socket)
        {
            Request("", socket, CommandConstants.ListGames);
            var bufferResponse = Response(socket, CommandConstants.ListGames);
            var lengthString = Encoding.UTF8.GetString(bufferResponse);
            var length = Convert.ToInt32(lengthString);
            gamesLoaded.Clear();
            Console.WriteLine("\n Games list: \n");
            for (int i = 0; i < length; i++)
            {
                bufferResponse = Response(socket, CommandConstants.ListGames);
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
                Console.WriteLine("search -> Search a game by its Title, Category, or Rating");
                Console.WriteLine("reviews -> Get reviews of a game");
                Console.WriteLine("rate -> Rate and comment a game");
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
                    case "search":
                        Search(socket);
                        break;
                    case "reviews":
                        GetReviews(socket);
                        break;
                    case "rate":
                        Rate(socket);
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

        private void Rate(Socket socket)
        {
            Console.WriteLine("\n Rate and comment a game");
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to rate and comment: ");
                var gameId = Console.ReadLine();
                var game = gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    string review = gameId + "*";
                    Console.WriteLine("Ingrese el rating en un rango del 1 al 5");
                    var ratingCorrecto = false;
                    while (!ratingCorrecto)
                    {
                        var insert = Console.ReadLine();
                        int rating = Convert.ToInt32(insert);
                        if (rating < 1 || rating > 5)
                        {
                            Console.WriteLine("Rating incorrecto, ingrese nuevamente. Debe estar entre 1 y 5");
                        }
                        else
                        {
                            review += insert + "*";
                            ratingCorrecto = true;
                        }
                    }
                    Console.WriteLine("Ingrese el comentario");
                    var comment = Console.ReadLine();
                    review += comment;
                    Request(review,socket,CommandConstants.Rate);
                    var bufferResponse = Response(socket, CommandConstants.Rate);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    idCorrecto = true;
                }
            }
        }

        private void GetReviews(Socket socket)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to get its reviews: ");
                var gameId = Console.ReadLine();
                var game = gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id doesnt exist");
                }
                else
                {
                    Request(gameId, socket, CommandConstants.GetReviews);
                    var bufferResponse = Response(socket, CommandConstants.GetReviews);
                    var lengthString = Encoding.UTF8.GetString(bufferResponse);
                    var length = Convert.ToInt32(lengthString);
                    Console.WriteLine("\n Game reviews: \n");
                    if (length == 0)
                    {
                        Console.WriteLine("The game doesnt have any reviews");
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            bufferResponse = Response(socket, CommandConstants.GetReviews);
                            string[] splittedReview = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                            string rating = splittedReview[0];
                            string comment = splittedReview[1];
                            Console.WriteLine($"{i+1}: Rating: {rating}");
                            Console.WriteLine($"{comment}");
                        }
                    }
                    idCorrecto = true;
                }
            }
        }

        private void Search(Socket socket)
        {
            var foundedGames = new List<Game>();
            Console.WriteLine("Insert some keywords to search a game: ");
            string keywords = Console.ReadLine();
            Request(keywords, socket, CommandConstants.Search);
            var bufferResponse = Response(socket, CommandConstants.Search);
            var length = Convert.ToInt32(Encoding.UTF8.GetString(bufferResponse));
            foundedGames.Clear();
            Console.WriteLine("\n Search result: \n");
            if (length == 0)
            {
                Console.WriteLine("0 games founded with the indicated parameters");
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    bufferResponse = Response(socket, CommandConstants.Search);
                    var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                    var game = new Game(split[1], split[2], split[4]);
                    game.Id = Convert.ToInt32(split[0]);
                    game.Rating = Convert.ToInt32(split[3]);
                    game.Image = split[5];
                    foundedGames.Add(game);
                    Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
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
                    var bufferResponse = Response(socket, CommandConstants.Purchase);
            
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

        private void Publish(Socket socket)
        {
            Console.WriteLine("\n Publish a game");
            var game = "";
            var atributes = new List<string>
            {
                "Title", "Genre", "Sinopsis"
            };
            for (int i = 0; i < atributes.Count; i++)
            {
                Console.Write($"\n Ingrese el {atributes[i]}:");
                var insert = Console.ReadLine();
                game += insert + "*";
            }
            Request(game,socket,CommandConstants.Publish);
            var bufferResponse = Response(socket, CommandConstants.Publish);
            
            Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
        }

        private byte[] Response(Socket socket, int command)
        {
            byte[] bufferResponse = new byte[] { };
            var headerLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                               HeaderConstants.DataLength;
            var buffer = new byte[headerLength];
            try
            {
                ReceiveData(socket, headerLength, buffer);
                var header = new Header();
                header.DecodeData(buffer);
                if (header.ICommand.Equals(command))
                {
                    bufferResponse = new byte[header.IDataLength];
                    ReceiveData(socket, header.IDataLength, bufferResponse);
                }
                else
                {
                    Console.WriteLine(header.ICommand + " " + command);
                }
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