﻿using System;
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
        private Socket _socket;
        
        public void Execute(Socket connectedSocket)
        {
            _socket = connectedSocket;
            Console.WriteLine("Welcome to the client system");
            Console.Write("Enter you username (in case it doesnt exists one will be created): ");
            var user = Console.ReadLine();
            try
            {
                Request(user, CommandConstants.Login);
                var bufferResponse = Response(CommandConstants.Login);

                Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));

                while (!_exit)
                {
                    Console.WriteLine("\n Options: ");
                    Console.WriteLine("list -> Visualize games list");
                    Console.WriteLine("publish -> Publish a game");
                    Console.WriteLine("publishedgames -> Visualize your published games list");
                    Console.WriteLine("message -> Send a message to the server");
                    Console.WriteLine("exit -> Quit the program");
                    Console.Write("Type your option: ");
                    var option = Console.ReadLine();
                    if (option != null)
                    {
                        switch (option.ToLower())
                        {
                            case "list":
                                ListGames();
                                break;
                            case "publish":
                                Publish();
                                break;
                            case "publishedgames":
                                ListPublishedGames();
                                break;
                            case "exit":
                                _socket.Shutdown(SocketShutdown.Both);
                                _socket.Close();
                                _exit = true;
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

        private void ListPublishedGames()
        {
            Request("", CommandConstants.ListPublishedGames);
            var bufferResponse = Response(CommandConstants.ListPublishedGames);
            var lengthString = Encoding.UTF8.GetString(bufferResponse);
            var length = Convert.ToInt32(lengthString);
            var gamesPublished = new List<Game>();
            Console.WriteLine("\n Published games list: \n");
            for (var i = 0; i < length; i++)
            {
                bufferResponse = Response(CommandConstants.ListPublishedGames);
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
                            ModifyGame(gamesPublished);
                            break;
                        case "modifyimage":
                            ModifyImage(gamesPublished);
                            break;
                        case "delete":
                            DeleteGame(gamesPublished);
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

        private void ModifyImage(List<Game> gamesPublished)
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to modify its image: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
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
                    Request(info,CommandConstants.ModifyImage);
                    SendFile(path, _socket);
                    Console.WriteLine("Sent");
                    var bufferResponse = Response(CommandConstants.ModifyImage);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    correctId = true;
                }
            }
        }

        private void Publish()
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
            Request(game,CommandConstants.Publish);
            
            var splittedGame = game.Split("*");
            var path = splittedGame[3];
            IFileHandler fileHandler = new FileHandler();
            while(path != null && path.Equals(string.Empty) && !fileHandler.FileExists(path))
            {
                path = Console.ReadLine();
            }
            SendFile(path, _socket);
            Console.WriteLine("Sent");

            var bufferResponse = Response(CommandConstants.Publish);
            Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
        }

        
        private void ModifyGame(List<Game> gamesPublished)
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to modify: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
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
                    Request(gameModified,CommandConstants.ModifyGame);
                    var bufferResponse = Response(CommandConstants.ModifyGame);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));

                    correctId = true;
                }
            }
        }

        private void DeleteGame(List<Game> gamesPublished)
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to delete: ");
                var gameId = Console.ReadLine();
                var game = gamesPublished.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    Request(gameId, CommandConstants.DeleteGame);
                    var bufferResponse = Response(CommandConstants.DeleteGame);
            
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    correctId = true;
                }
            }
        }

        private void ListGames()
        {
            Request("", CommandConstants.ListGames);
            var bufferResponse = Response(CommandConstants.ListGames);
            var lengthString = Encoding.UTF8.GetString(bufferResponse);
            var length = Convert.ToInt32(lengthString);
            _gamesLoaded.Clear();
            Console.WriteLine("\n Games list: \n");
            for (var i = 0; i < length; i++)
            {
                bufferResponse = Response(CommandConstants.ListGames);
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
                Console.WriteLine("purchasedgames -> List the games already purchased by the user");
                Console.WriteLine("search -> Search a game by its Title, Category");
                Console.WriteLine("filterrating -> Filter games by a minimum rating");
                Console.WriteLine("reviews -> Get reviews of a game");
                Console.WriteLine("rate -> Rate and comment a game");
                Console.WriteLine("download -> Download the image of a game");
                Console.WriteLine("main <- Go to main menu");
                Console.Write("Option: ");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "detail":
                        DetailGame();
                        break;
                    case "purchase":
                        Purchase();
                        break;
                    case "purchasedgames":
                        GetPurchasedGames();
                        break;
                    case "search":
                        Search();
                        break;
                    case "filterrating":
                        SearchRating();
                        break;
                    case "reviews":
                        GetReviews();
                        break;
                    case "rate":
                        Rate();
                        break;
                    case "download":
                        Download();
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

        private void GetPurchasedGames()
        {
            Request("", CommandConstants.ListPurchasedGames);
            var bufferResponse = Response(CommandConstants.ListPurchasedGames);
            var lengthString = Encoding.UTF8.GetString(bufferResponse);
            var length = Convert.ToInt32(lengthString);
            var gamesPurchased = new List<Game>();
            Console.WriteLine("\n Purchased games list: \n");
            for (var i = 0; i < length; i++)
            {
                bufferResponse = Response(CommandConstants.ListPurchasedGames);
                var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                var game = new Game(split[1], split[2], split[4], split[5])
                {
                    Id = Convert.ToInt32(split[0]),
                    Rating = Convert.ToInt32(split[3])
                };
                gamesPurchased.Add(game);
                Console.WriteLine($"{game.Id}: {game.Title} - {game.Genre} - {game.Rating}\n");
            }
        }

        private void SendParameters(string parameter, int command)
        {
            Request(parameter, command);
            var bufferResponse = Response(command);
            var length = Convert.ToInt32(Encoding.UTF8.GetString(bufferResponse));

            _gamesLoaded.Clear();
            Console.WriteLine("\n Search result: \n");
            if (length == 0)
            {
                Console.WriteLine("0 games found with the indicated parameters");
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    bufferResponse = Response(command);
                    var split = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                    var game = new Game(split[1], split[2], split[4], split[5])
                    {
                        Id = Convert.ToInt32(split[0]),
                        Rating = Convert.ToInt32(split[3])
                    };
                    _gamesLoaded.Add(game);
                    Console.WriteLine($"{game.Id}. {game.Title} - {game.Genre} - {game.Rating}\n");
                }
            }
        }

        private void SearchRating()
        {
            Console.WriteLine("Insert a minimum rating to filter games: ");
            var minimumRating = Console.ReadLine();
            SendParameters(minimumRating, CommandConstants.FilterRating);
        }

        private void Search()
        {
            Console.WriteLine("Insert some keywords to search a game: ");
            var keywords = Console.ReadLine();
            SendParameters(keywords,CommandConstants.Search);
        }

        private void Download()
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to download its image: ");
                var gameId = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    Request(game.Id.ToString(),CommandConstants.Download);
                    var bufferResponse = Response(CommandConstants.Download);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    
                    ReceiveFile();
                    Console.WriteLine("File received");
                    correctId = true;
                }
            }
        }

        private void Rate()
        {
            Console.WriteLine("\n Rate and comment a game");
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to rate and comment: ");
                var gameId = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    var review = gameId + "*";
                    Console.WriteLine("Type the rating in a 1 to 5 range");
                    var correctRating = false;
                    while (!correctRating)
                    {
                        var insert = Console.ReadLine();
                        int rating = Convert.ToInt32(insert);
                        if (rating < 1 || rating > 5)
                        {
                            Console.WriteLine("Incorrect rating, try again. It must be in the 1 to 5 range");
                        }
                        else
                        {
                            review += insert + "*";
                            correctRating = true;
                        }
                    }
                    Console.WriteLine("Type your comment");
                    var comment = Console.ReadLine();
                    review += comment;
                    Request(review,CommandConstants.Rate);
                    var bufferResponse = Response(CommandConstants.Rate);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    correctId = true;
                }
            }
        }

        private void GetReviews()
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to get its reviews: ");
                var gameId = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    Request(gameId, CommandConstants.GetReviews);
                    var bufferResponse = Response(CommandConstants.GetReviews);
                    var lengthString = Encoding.UTF8.GetString(bufferResponse);
                    var length = Convert.ToInt32(lengthString);
                    Console.WriteLine("\n Game reviews: \n");
                    if (length == 0)
                    {
                        Console.WriteLine("The game doesnt have any reviews");
                    }
                    else
                    {
                        for (var i = 0; i < length; i++)
                        {
                            bufferResponse = Response(CommandConstants.GetReviews);
                            var splittedReview = (Encoding.UTF8.GetString(bufferResponse)).Split("*");
                            var rating = splittedReview[0];
                            var comment = splittedReview[1];
                            Console.WriteLine($"{i+1}: Rating: {rating}");
                            Console.WriteLine($"{comment}");
                        }
                    }
                    correctId = true;
                }
            }
        }

        private void Purchase()
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to purchase: ");
                var gameId = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(gameId)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    Request(gameId, CommandConstants.Purchase);
                    var bufferResponse = Response(CommandConstants.Purchase);
            
                    Console.WriteLine(Encoding.UTF8.GetString(bufferResponse));
                    correctId = true;
                }
            }
        }

        private void DetailGame()
        {
            var correctId = false;
            while (!correctId)
            {
                Console.Write("Insert the id of the game to select: ");
                var id = Console.ReadLine();
                var game = _gamesLoaded.Find(e => e.Id.Equals(Convert.ToInt32(id)));
                if (game == null)
                {
                    Console.WriteLine("Did not found any game with the selected id");
                }
                else
                {
                    Request(id,CommandConstants.DetailGame);
                    var bufferResponse = Response(CommandConstants.DetailGame);
                    var responseString = Encoding.UTF8.GetString(bufferResponse);
                    if (responseString.Contains("*"))
                    {
                        var split = (responseString).Split("*");

                        Console.WriteLine($" --- {split[1]} --- ");
                        Console.WriteLine($"{split[2]} *{split[3]}");
                        Console.WriteLine(split[4]);
                        Console.WriteLine($"Image file name: {split[5]}");
                    }
                    else
                    {
                        Console.WriteLine(responseString);
                    }

                    correctId = true;
                }
            }
        }

        private byte[] Response(int command)
        {
            var bufferResponse = new byte[] { };
            var headerLength = HeaderConstants.Response.Length + HeaderConstants.CommandLength +
                               HeaderConstants.DataLength;
            var buffer = new byte[headerLength];
            try
            {
                ReceiveData(headerLength, buffer);
                var header = new Header();
                header.DecodeData(buffer);
                if (header.ICommand.Equals(command))
                {
                    bufferResponse = new byte[header.IDataLength];
                    ReceiveData(header.IDataLength, bufferResponse);
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

        private void Request(string mensaje, int command)
        {
            var header = new Header(HeaderConstants.Request, command, mensaje.Length);
            var data = header.GetRequest();
            var sentBytes = 0;
            try
            {
                while (sentBytes < data.Length)
                {
                    sentBytes += _socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
                }

                sentBytes = 0;
                var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
                while (sentBytes < bytesMessage.Length)
                {
                    sentBytes += _socket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                        SocketFlags.None);
                }
            } 
            catch (SocketException)
            {
                throw new ServerDisconnected();
            }
        }
        
        private void ReceiveData(int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                var localRecv = _socket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    
                iRecv += localRecv;
            }
        }
        
        private void SendFile(string path, Socket socket)
        {
            var fileName = _fileHandler.GetFileName(path); // nombre del archivo -> XXXX
            var fileSize = _fileHandler.GetFileSize(path); // tamaño del archivo -> YYYYYYYY
            var header = new Header().Create(fileName, fileSize);
            socket.Send(header, header.Length, SocketFlags.None);
            
            var fileNameToBytes = Encoding.UTF8.GetBytes(fileName);
            socket.Send(fileNameToBytes, fileNameToBytes.Length, SocketFlags.None);
            
            var parts = Header.GetParts(fileSize);
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

        private void ReceiveFile()
        {
            var fileHeader = new byte[Header.GetLength()];
            ReceiveData(Header.GetLength(), fileHeader);
            var fileNameSize = BitConverter.ToInt32(fileHeader, 0);
            var fileSize = BitConverter.ToInt64(fileHeader, HeaderConstants.FixedFileNameLength);
            
            var bufferName = new byte[fileNameSize];
            ReceiveData(fileNameSize, bufferName);
            var fileName = Encoding.UTF8.GetString(bufferName);
            
            var parts = Header.GetParts(fileSize);
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
                    ReceiveData(lastPartSize, data);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine(
                        $"Will receive segment number {currentPart} with size {HeaderConstants.MaxPacketSize}");
                    data = new byte[HeaderConstants.MaxPacketSize];
                    ReceiveData(HeaderConstants.MaxPacketSize, data);
                    offset += HeaderConstants.MaxPacketSize;
                }

                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }

    }
}