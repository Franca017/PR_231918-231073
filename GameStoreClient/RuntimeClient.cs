using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Domain;
using Domain.Exceptions;
using ProtocolLibrary;
using ProtocolLibrary.FileHandler;
using ProtocolLibrary.FileHandler.Interfaces;

namespace GameStoreClient
{
    public class Runtime
    {
        private bool _exit;
        private readonly List<Game> _gamesLoaded = new List<Game>();
        private readonly IFileStreamHandler _fileStreamHandler = new FileStreamHandler();
        private readonly IFileHandler _fileHandler = new FileHandler();
        
        public void Execute(Socket socket)
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.Write("Ingrese su nombre de Usuario (en caso de no existir se le creara uno): ");
            var user = Console.ReadLine();
            try
            {
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
                    {
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
                                Console.WriteLine("Invalid option");
                                break;
                        }
                    }
                }
            }
            catch (ServerDisconnected s)
            {
                _exit = true;
                Console.WriteLine(s.Message);
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
            for (var i = 0; i < length; i++)
            {
                bufferResponse = Response(socket, CommandConstants.ListPublishedGames);
                var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                var game = new Game(split[1], split[2], split[4], split[5])
                {
                    Id = Convert.ToInt32(split[0]),
                    Rating = Convert.ToInt32(split[3]),
                    Image = split[5]
                };
                gamesPublished.Add(game);
                Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
            }
            
            var main = false;
            while (!main)
            {
                Console.WriteLine("\n Options:");
                Console.WriteLine("modify -> Modify an existing game");
                Console.WriteLine("modifyimage -> Modify an existing game image");
                Console.WriteLine("delete -> Delete a game");
                Console.WriteLine("main <- Go to main menu");
                Console.Write("Option: ");
                var option = Console.ReadLine();
                if (option != null)
                    switch (option.ToLower())
                    {
                        case "modify":
                            ModifyGame(socket, gamesPublished);
                            break;
                        case "modifyimage":
                            ModifyImage(socket, gamesPublished);
                            break;
                        case "delete":
                            DeleteGame(socket, gamesPublished);
                            break;
                        case "main":
                            main = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option");
                            break;
                    }
            }
        }

        private void ModifyImage(Socket socket, List<Game> gamesPublished)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to modify its image: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Console.WriteLine("Insert the path of the new file:");
                    var path = String.Empty;
                    IFileHandler fileHandler = new FileHandler();
                    while(path != null && path.Equals(string.Empty) && !fileHandler.FileExists(path))
                    {
                        path = Console.ReadLine();
                    }

                    var info = gameId + "*" + path;
                    Request(info,socket,CommandConstants.ModifyImage);
                    SendFile(path, socket);
                    Console.WriteLine("Enviado");
                    var bufferResponse = Response(socket, CommandConstants.ModifyImage);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    idCorrecto = true;
                }
            }
        }

        private void Publish(Socket socket)
        {
            Console.WriteLine("\n Publish a game");
            var game = "";
            var attributes = new List<string>
            {
                "Title", "Genre", "Sinopsis", "Path of the image"
            };
            foreach (var attribute in attributes)
            {
                Console.Write($"\n  Insert the {attribute}:");
                var insert = Console.ReadLine();
                game += insert + "*";
            }
            Request(game,socket,CommandConstants.Publish);
            // --
            var splittedGame = game.Split("*");
            var path = splittedGame[3];
            IFileHandler fileHandler = new FileHandler();
            while(path != null && path.Equals(string.Empty) && !fileHandler.FileExists(path))
            {
                path = Console.ReadLine();
            }
            SendFile(path, socket);
            Console.WriteLine("Enviado");
            // --
            var bufferResponse = Response(socket, CommandConstants.Publish);
            
            Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
        }

        
        private void ModifyGame(Socket socket, List<Game> gamesPublished)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to modify: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    var gameModified = gameId+"*";
                    var attributes = new List<string>
                    {
                        "Title", "Genre", "Sinopsis"
                    };
                    foreach (var attribute in attributes)
                    {
                        Console.Write($"\n Insert the new {attribute} (in case of not modifying it, leave empty):");
                        var insert = Console.ReadLine();
                        if (insert is "")
                        {
                            gameModified += "-" + "*";
                        }
                        else
                        {
                            gameModified += insert + "*";
                        }
                    }
                    Request(gameModified,socket,CommandConstants.ModifyGame);
                    var bufferResponse = Response(socket, CommandConstants.ModifyGame);
            
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));

                    idCorrecto = true;
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
            _gamesLoaded.Clear();
            Console.WriteLine("\n Games list: \n");
            for (var i = 0; i < length; i++)
            {
                bufferResponse = Response(socket, CommandConstants.ListGames);
                var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                var game = new Game(split[1], split[2], split[4], split[5])
                {
                    Id = Convert.ToInt32(split[0]),
                    Rating = Convert.ToInt32(split[3]),
                    Image = split[5]
                };
                Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
                _gamesLoaded.Add(game);
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
                Console.WriteLine("download -> Download the image of a game");
                Console.WriteLine("main <- Go to main menu");
                Console.Write("Option: ");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "detail":
                        DetailGame(socket);
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
                    case "download":
                        Download(socket);
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

        private void Download(Socket socket)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to download its image: ");
                var gameId = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Request(gameId.ToString(),socket,CommandConstants.Download);
                    var bufferResponse = Response(socket, CommandConstants.Download);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    
                    ReceiveFile(socket);
                    Console.WriteLine("File received");
                    idCorrecto = true;
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
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
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
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
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
            Console.WriteLine("Insert some keywords to search a game: ");
            var keywords = Console.ReadLine();
            Request(keywords, socket, CommandConstants.Search);
            var bufferResponse = Response(socket, CommandConstants.Search);
            var length = Convert.ToInt32(Encoding.UTF8.GetString(bufferResponse));
            
            var gamesFound = new List<Game>();
            Console.WriteLine("\n Search result: \n");
            if (length == 0)
            {
                Console.WriteLine("0 games founded with the indicated parameters");
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    bufferResponse = Response(socket, CommandConstants.Search);
                    var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                    var game = new Game(split[1], split[2], split[4], split[5])
                    {
                        Id = Convert.ToInt32(split[0]),
                        Rating = Convert.ToInt32(split[3]),
                        Image = split[5]
                    };
                    gamesFound.Add(game);
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
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
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

        private void DetailGame(Socket socket)
        {
            var idCorrecto = false;
            while (!idCorrecto)
            {
                Console.Write("Insert the id of the game to select: ");
                var id = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(id)));
                if (game == null)
                {
                    Console.WriteLine("Id inexistente");
                }
                else
                {
                    Request(id,socket,CommandConstants.DetailGame);
                    var bufferResponse = Response(socket, CommandConstants.DetailGame);
                    var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                    
                    Console.WriteLine($" --- {split[1]} --- ");
                    Console.WriteLine($"{split[2]} *{split[3]}");
                    Console.WriteLine(split[4]);
                    Console.WriteLine($"Image file name: {split[5]}");
                    idCorrecto = true;
                }
            }
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
            catch (SocketException)
            {
                throw new ServerDisconnected();
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
            try
            {
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
            catch (SocketException)
            {
                throw new ServerDisconnected();
            }
        }
        
        private void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                var localRecv = clientSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    
                iRecv += localRecv;
            }
        }
        
        // --
        private void SendFile(string path, Socket socket)
        {
            var fileName = _fileHandler.GetFileName(path); // nombre del archivo -> XXXX
            var fileSize = _fileHandler.GetFileSize(path); // tamaño del archivo -> YYYYYYYY
            var header = new Header().Create(fileName, fileSize);
            socket.Send(header, header.Length, SocketFlags.None);
            
            var fileNameToBytes = Encoding.UTF8.GetBytes(fileName);
            socket.Send(fileNameToBytes, fileNameToBytes.Length, SocketFlags.None);
            
            long parts = Header.GetParts(fileSize);
            Console.WriteLine("Will Send {0} parts",parts);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, HeaderConstants.MaxPacketSize);
                    offset += HeaderConstants.MaxPacketSize;
                }
                socket.Send(data, data.Length, SocketFlags.None);
                currentPart++;
            }
        }
        // --
        
        public void ReceiveFile(Socket socket)
        {
            var fileHeader = new byte[Header.GetLength()];
            ReceiveData(socket, Header.GetLength(), fileHeader);
            var fileNameSize = BitConverter.ToInt32(fileHeader, 0);
            var fileSize = BitConverter.ToInt64(fileHeader, HeaderConstants.FixedFileNameLength);
            
            var bufferName = new byte[fileNameSize];
            ReceiveData(socket, fileNameSize, bufferName);
            var fileName = Encoding.UTF8.GetString(bufferName);
            
            long parts = Header.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            Console.WriteLine(
                $"Will receive file {fileName} with size {fileSize} that will be received in {parts} segments");
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int) (fileSize - offset);
                    Console.WriteLine($"Will receive segment number {currentPart} with size {lastPartSize}");
                    data = new byte[lastPartSize];
                    ReceiveData(socket, lastPartSize, data);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine(
                        $"Will receive segment number {currentPart} with size {HeaderConstants.MaxPacketSize}");
                    data = new byte[HeaderConstants.MaxPacketSize];
                    ReceiveData(socket, HeaderConstants.MaxPacketSize, data);
                    offset += HeaderConstants.MaxPacketSize;
                }

                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }

    }
}