using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Domain.Exceptions;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;
using ProtocolLibrary.FileHandler;
using ProtocolLibrary.FileHandler.Interfaces;
using RabbitMQ.Client;
using IModel = Microsoft.EntityFrameworkCore.Metadata.IModel;

namespace GameStoreServer
{
    public class Runtime
    {
        private bool Exit { get; set; }

        private readonly TcpClient _connectedClient;
        private readonly NetworkStream _networkStream;

        private readonly IGamesLogic _gamesLogic;
        private readonly IUserLogic _userLogic;
        private readonly IReviewLogic _reviewLogic;
        private readonly IFileStreamHandler _fileStreamHandler;
        private readonly IFileHandler _fileHandler;
        private RabbitMQ.Client.IModel _channel;

        private User _userLogged;

        public Runtime(IServiceProvider serviceProvider, TcpClient clientConnected)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
            _fileStreamHandler = new FileStreamHandler();
            _fileHandler = new FileHandler();
            _connectedClient = clientConnected;
            _networkStream = clientConnected.GetStream();
            _channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            _channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public async void HandleConnectionAsync()
        {
                while (!Exit)
                {
                    var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                       HeaderConstants.DataLength;
                    var buffer = new byte[headerLength];
                    try
                    {
                        await ReceiveDataAsync(headerLength, buffer);
                        var header = new Header();
                        header.DecodeData(buffer);
                        switch (header.ICommand)
                        {
                            case CommandConstants.Login:
                                await LoginAsync(header);
                                break;
                            case CommandConstants.ListGames:
                                await ListGamesAsync(header);
                                break;
                            case CommandConstants.DetailGame:
                                await DetailGameAsync(header);
                                break;
                            case CommandConstants.Purchase:
                                await PurchaseAsync(header);
                                break;
                            case CommandConstants.GetReviews:
                                await GetReviewsAsync(header);
                                break;
                            case CommandConstants.Publish:
                                await PublishAsync(header);
                                break;
                            case CommandConstants.Search:
                                await SearchAsync(header);
                                break;
                            case CommandConstants.FilterRating:
                                await FilterRatingAsync(header);
                                break;
                            case CommandConstants.ListPublishedGames:
                                await ListPublishedGamesAsync(header);
                                break;
                            case CommandConstants.ModifyGame:
                                await ModifyGameAsync(header);
                                break;
                            case CommandConstants.DeleteGame:
                                await DeleteGameAsync(header);
                                break;
                            case CommandConstants.Rate:
                                await RateAsync(header);
                                break;
                            case CommandConstants.Download:
                                await DownloadAsync(header);
                                break;
                            case CommandConstants.ModifyImage:
                                await ModifyImageAsync(header);
                                break;
                            case CommandConstants.ListPurchasedGames:
                                await GetPurchasedGamesAsync(header);
                                break;
                        }
                    }
                    catch (ClientDisconnected c)
                    {
                        Console.WriteLine(_userLogged != null
                            ? $"{c.Message} {_userLogged.UserName} disconnected"
                            : $"{c.Message} Unlogged user disconnected");
                        Exit = true;
                    }
                }
        }

        private async Task GetPurchasedGamesAsync(Header header)
        {
            var purchasedGames = _userLogic.GetPurchasedGames(_userLogged.Id);
            await ResponseAsync(purchasedGames.Count.ToString(),CommandConstants.ListPurchasedGames);
            
            await SendGamesAsync(header,purchasedGames);
        }

        private async Task SendGamesAsync(Header header, List<Game> gamesFound)
        {
            foreach (var game in gamesFound)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                await ResponseAsync(gameToString, header.ICommand);
            }
        }

        private async Task FilterRatingAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var minRating = Convert.ToInt32(Encoding.UTF8.GetString(bufferData));
            var gamesFound = _gamesLogic.GetGamesOverRating(minRating);
            await ResponseAsync(gamesFound.Count.ToString(), CommandConstants.FilterRating);
            
            await SendGamesAsync(header, gamesFound);
        }

        private async Task SearchAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var keywords = Encoding.UTF8.GetString(bufferData);
            var gamesFound = _gamesLogic.GetSearchedGames(keywords);
            await ResponseAsync(gamesFound.Count.ToString(), CommandConstants.Search);
            
            await SendGamesAsync(header,gamesFound);
        }

        private async Task DownloadAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            await ResponseAsync($"The image of the game with id {gameId} is going to be sent", header.ICommand);
            var game = _gamesLogic.GetById(gameId);
            var path = game.Image;
            await SendFileAsync(path);
        }

        private async Task RateAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);

            var splittedReview = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameId = Convert.ToInt32(splittedReview[0]);
            var reviewedGame = _gamesLogic.GetById(gameId);
            var newReview = new Review(_userLogged, reviewedGame, Convert.ToInt32(splittedReview[1]),
                splittedReview[2]);
            _reviewLogic.Add(newReview);
            _reviewLogic.AdjustRating(gameId);
            var response = $"{_userLogged.UserName} successfully reviewed {reviewedGame.Title}";
            await ResponseAsync(response, header.ICommand);
        }

        private async Task DeleteGameAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);

            var gameId = Convert.ToInt32(gameIdString);
            _gamesLogic.Delete(gameId);
            await ResponseAsync($"Your game with id {gameId} was deleted.", header.ICommand);
        }

        private async Task ModifyGameAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.Modify(modifySplit);
            await ResponseAsync($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }
        
        private async Task ModifyImageAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            await ReceiveFileAsync();
            Console.WriteLine("File received");
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.ModifyImage(modifySplit);
            await ResponseAsync($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }

        private async Task ListPublishedGamesAsync(Header header)
        {
            var listPublished = _gamesLogic.GetPublishedGames(_userLogged);
            await ResponseAsync(listPublished.Count.ToString(),CommandConstants.ListPublishedGames);
            
            await SendGamesAsync(header,listPublished);
        }

        private async Task PublishAsync(Header header)
        {
            var bufferPublish = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferPublish);
            await ReceiveFileAsync();
            Console.WriteLine("File received");
            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
            var newGame = new Game(split[0], split[1], split[2], split[3])
            {
                Creator = _userLogged
            };
            var newGameInDb = _gamesLogic.Add(newGame);

            var response = $"{newGameInDb.Title} was published to the store with id {newGameInDb.Id}";
            await ResponseAsync(response, header.ICommand);
        }

        private async Task GetReviewsAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var reviewsList = _reviewLogic.GetGameReviews(gameId);
            await ResponseAsync(reviewsList.Count.ToString(), CommandConstants.GetReviews);
            foreach (var review in reviewsList)
            {
                var reviewToString = review.Rating + "*" + review.Comment;
                await ResponseAsync(reviewToString, header.ICommand);
            }
        }

        private async Task PurchaseAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var response = _userLogic.PurchaseGame(_userLogged, gameId);
            
            await ResponseAsync(response, header.ICommand);
        }

        private async Task DetailGameAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);

            var detailedGame = _gamesLogic.GetById(gameId);
            if (detailedGame != null)
            {
                var gameToString =
                    $"{detailedGame.Id}*{detailedGame.Title}*{detailedGame.Genre}*{detailedGame.Rating}*{detailedGame.Sinopsis}*{detailedGame.Image}";
                await ResponseAsync(gameToString, header.ICommand);
            }
            else
            {
                await ResponseAsync("The game selected has been deleted from the store.", header.ICommand);
            }
        }

        private async Task ListGamesAsync(Header header)
        {
            var list = _gamesLogic.GetAll();
            await ResponseAsync(list.Count.ToString(), CommandConstants.ListGames);
            
            await SendGamesAsync(header,list);
            Log newLog = new Log("GTA", "Pedro", DateTime.Now, "List", "Pedro pidio los juegos");
            bool result = await SendLog(newLog);
            Console.WriteLine(result);
        }

        private async Task LoginAsync(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(header.IDataLength, bufferData);
            var user = Encoding.UTF8.GetString(bufferData);
            Console.WriteLine("User: " + Encoding.UTF8.GetString(bufferData));
            var userInDb = _userLogic.Login(user);
            _userLogged = userInDb;
            var response =
                $"Log in of the user: {userInDb.UserName} (created at {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month}).";
            await ResponseAsync(response, header.ICommand);
        }


        private async Task ResponseAsync(string mensaje, int command)
        {
            var header = new Header(HeaderConstants.Request, command, mensaje.Length);
            var data = header.GetRequest();
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            await _networkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await _networkStream.WriteAsync(bytesMessage, 0, bytesMessage.Length).ConfigureAwait(false);
        }

        private async Task ReceiveDataAsync(int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var received = await _networkStream
                        .ReadAsync(buffer, iRecv, length - iRecv)
                        .ConfigureAwait(false);

                    if (received == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!Exit)
                        {
                            _connectedClient.Close();
                            _networkStream.Close();
                            throw new ClientDisconnected();
                        }
                        else
                        {
                            throw new Exception("Server is closing");
                        }
                    }

                    iRecv += received;
                }
                catch (IOException se)
                {
                    Console.WriteLine(se.Message);
                    throw new ClientDisconnected();
                }
            }
        }

        private async Task ReceiveFileAsync()
        {
            var fileHeader = new byte[Header.GetLength()];
            await ReceiveDataAsync(Header.GetLength(), fileHeader);
            var fileNameSize = BitConverter.ToInt32(fileHeader, 0);
            var fileSize = BitConverter.ToInt64(fileHeader, HeaderConstants.FixedFileNameLength);
            
            var bufferName = new byte[fileNameSize];
            await ReceiveDataAsync(fileNameSize, bufferName);
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
                    await ReceiveDataAsync(lastPartSize, data);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine(
                        $"Will receive segment number {currentPart} with size {HeaderConstants.MaxPacketSize}");
                    data = new byte[HeaderConstants.MaxPacketSize];
                    await ReceiveDataAsync(HeaderConstants.MaxPacketSize, data);
                    offset += HeaderConstants.MaxPacketSize;
                }

                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
        
        private async Task SendFileAsync(string path)
        {
            var fileName = _fileHandler.GetFileName(path);
            var fileSize = _fileHandler.GetFileSize(path);
            var header = new Header().Create(fileName, fileSize);
            await _networkStream.WriteAsync(header, 0, header.Length);
            
            var fileNameToBytes = Encoding.UTF8.GetBytes(fileName);
            await _networkStream.WriteAsync(fileNameToBytes, 0, fileNameToBytes.Length);
            
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
                await _networkStream.WriteAsync(data, 0, data.Length);
                currentPart++;
            }
        }

        private Task<bool> SendLog(Log log)
        {
            var stringLog = JsonSerializer.Serialize(log);
            bool returnVal;
            try
            {
                var body = Encoding.UTF8.GetBytes(stringLog);
                _channel.BasicPublish(exchange: "",
                    routingKey: "log_queue",
                    basicProperties: null,
                    body: body);
                returnVal = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnVal = false;
            }

            return Task.FromResult(returnVal);
        }
    }
}