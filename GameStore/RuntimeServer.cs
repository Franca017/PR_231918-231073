using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Exceptions;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;
using ProtocolLibrary.FileHandler;
using ProtocolLibrary.FileHandler.Interfaces;

namespace GameStoreServer
{
    public class Runtime
    {
        private bool Exit { get; set; }

        private readonly TcpClient _connectedSocket;
        private readonly NetworkStream _networkStream;

        private readonly IGamesLogic _gamesLogic;
        private readonly IUserLogic _userLogic;
        private readonly IReviewLogic _reviewLogic;
        private readonly IFileStreamHandler _fileStreamHandler;
        private readonly IFileHandler _fileHandler;

        private User _userLogged;

        public Runtime(IServiceProvider serviceProvider, TcpClient clientConnected)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
            _fileStreamHandler = new FileStreamHandler();
            _fileHandler = new FileHandler();
            _connectedSocket = clientConnected;
            _networkStream = clientConnected.GetStream();
        }

        public async void HandleConnection()
        {
                while (!Exit)
                {
                    var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                       HeaderConstants.DataLength;
                    var buffer = new byte[headerLength];
                    try
                    {
                        await ReceiveData(headerLength, buffer);
                        var header = new Header();
                        header.DecodeData(buffer);
                        switch (header.ICommand)
                        {
                            case CommandConstants.Login:
                                await Login(header);
                                break;
                            case CommandConstants.ListGames:
                                await ListGames(header);
                                break;
                            case CommandConstants.DetailGame:
                                await DetailGame(header);
                                break;
                            case CommandConstants.Purchase:
                                await Purchase(header);
                                break;
                            case CommandConstants.GetReviews:
                                await GetReviews(header);
                                break;
                            case CommandConstants.Publish:
                                await Publish(header);
                                break;
                            case CommandConstants.Search:
                                await Search(header);
                                break;
                            case CommandConstants.FilterRating:
                                await FilterRating(header);
                                break;
                            case CommandConstants.ListPublishedGames:
                                await ListPublishedGames(header);
                                break;
                            case CommandConstants.ModifyGame:
                                await ModifyGame(header);
                                break;
                            case CommandConstants.DeleteGame:
                                await DeleteGame(header);
                                break;
                            case CommandConstants.Rate:
                                await Rate(header);
                                break;
                            case CommandConstants.Download:
                                await Download(header);
                                break;
                            case CommandConstants.ModifyImage:
                                await ModifyImage(header);
                                break;
                            case CommandConstants.ListPurchasedGames:
                                await GetPurchasedGames(header);
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

        private async Task GetPurchasedGames(Header header)
        {
            var purchasedGames = _userLogic.GetPurchasedGames(_userLogged.Id);
            await Response(purchasedGames.Count.ToString(),CommandConstants.ListPurchasedGames);
            
            await SendGames(header,purchasedGames);
        }

        private async Task SendGames(Header header, List<Game> gamesFound)
        {
            foreach (var game in gamesFound)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                await Response(gameToString, header.ICommand);
            }
        }

        private async Task FilterRating(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var minRating = Convert.ToInt32(Encoding.UTF8.GetString(bufferData));
            var gamesFound = _gamesLogic.GetGamesOverRating(minRating);
            await Response(gamesFound.Count.ToString(), CommandConstants.FilterRating);
            
            await SendGames(header, gamesFound);
        }

        private async Task Search(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var keywords = Encoding.UTF8.GetString(bufferData);
            var gamesFound = _gamesLogic.GetSearchedGames(keywords);
            await Response(gamesFound.Count.ToString(), CommandConstants.Search);
            
            await SendGames(header,gamesFound);
        }

        private async Task Download(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            await Response($"The image of the game with id {gameId} is going to be sent", header.ICommand);
            var game = _gamesLogic.GetById(gameId);
            var path = game.Image;
            await SendFile(path);
        }

        private async Task Rate(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);

            var splittedReview = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameId = Convert.ToInt32(splittedReview[0]);
            var reviewedGame = _gamesLogic.GetById(gameId);
            var newReview = new Review(_userLogged, reviewedGame, Convert.ToInt32(splittedReview[1]),
                splittedReview[2]);
            _reviewLogic.Add(newReview);
            _reviewLogic.AdjustRating(gameId);
            var response = $"{_userLogged.UserName} successfully reviewed {reviewedGame.Title}";
            await Response(response, header.ICommand);
        }

        private async Task DeleteGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);

            var gameId = Convert.ToInt32(gameIdString);
            _gamesLogic.Delete(gameId);
            await Response($"Your game with id {gameId} was deleted.", header.ICommand);
        }

        private async Task ModifyGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.Modify(modifySplit);
            await Response($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }
        
        private async Task ModifyImage(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            await ReceiveFile();
            Console.WriteLine("File received");
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.ModifyImage(modifySplit);
            await Response($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }

        private async Task ListPublishedGames(Header header)
        {
            var listPublished = _gamesLogic.GetPublishedGames(_userLogged);
            await Response(listPublished.Count.ToString(),CommandConstants.ListPublishedGames);
            
            await SendGames(header,listPublished);
        }

        private async Task Publish(Header header)
        {
            var bufferPublish = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferPublish);
            await ReceiveFile();
            Console.WriteLine("File received");
            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
            var newGame = new Game(split[0], split[1], split[2], split[3])
            {
                Creator = _userLogged
            };
            var newGameInDb = _gamesLogic.Add(newGame);

            var response = $"{newGameInDb.Title} was published to the store with id {newGameInDb.Id}";
            await Response(response, header.ICommand);
        }

        private async Task GetReviews(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var reviewsList = _reviewLogic.GetGameReviews(gameId);
            await Response(reviewsList.Count.ToString(), CommandConstants.GetReviews);
            foreach (var review in reviewsList)
            {
                var reviewToString = review.Rating + "*" + review.Comment;
                await Response(reviewToString, header.ICommand);
            }
        }

        private async Task Purchase(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var response = _userLogic.PurchaseGame(_userLogged, gameId);
            
            await Response(response, header.ICommand);
        }

        private async Task DetailGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);

            var detailedGame = _gamesLogic.GetById(gameId);
            if (detailedGame != null)
            {
                var gameToString =
                    $"{detailedGame.Id}*{detailedGame.Title}*{detailedGame.Genre}*{detailedGame.Rating}*{detailedGame.Sinopsis}*{detailedGame.Image}";
                await Response(gameToString, header.ICommand);
            }
            else
            {
                await Response("The game selected has been deleted from the store.", header.ICommand);
            }
        }

        private async Task ListGames(Header header)
        {
            var list = _gamesLogic.GetAll();
            await Response(list.Count.ToString(), CommandConstants.ListGames);
            
            await SendGames(header,list);
        }

        private async Task Login(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            await ReceiveData(header.IDataLength, bufferData);
            var user = Encoding.UTF8.GetString(bufferData);
            Console.WriteLine("User: " + Encoding.UTF8.GetString(bufferData));
            var userInDb = _userLogic.Login(user);
            _userLogged = userInDb;
            var response =
                $"Log in of the user: {userInDb.UserName} (created at {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month}).";
            await Response(response, header.ICommand);
        }


        private async Task Response(string mensaje, int command)
        {
            var header = new Header(HeaderConstants.Request, command, mensaje.Length);
            var data = header.GetRequest();
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            await _networkStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await _networkStream.WriteAsync(bytesMessage, 0, bytesMessage.Length).ConfigureAwait(false);
        }

        private async Task ReceiveData(int length, byte[] buffer)
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
                            //_connectedSocket.Shutdown(SocketShutdown.Both);
                            //_connectedSocket.Close();
                            throw new ClientDisconnected();
                        }
                        else
                        {
                            throw new Exception("Server is closing");
                        }
                    }

                    iRecv += received;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    throw new ClientDisconnected();
                }
            }
        }

        private async Task ReceiveFile()
        {
            var fileHeader = new byte[Header.GetLength()];
            await ReceiveData(Header.GetLength(), fileHeader);
            var fileNameSize = BitConverter.ToInt32(fileHeader, 0);
            var fileSize = BitConverter.ToInt64(fileHeader, HeaderConstants.FixedFileNameLength);
            
            var bufferName = new byte[fileNameSize];
            await ReceiveData(fileNameSize, bufferName);
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
                    await ReceiveData(lastPartSize, data);
                    offset += lastPartSize;
                }
                else
                {
                    Console.WriteLine(
                        $"Will receive segment number {currentPart} with size {HeaderConstants.MaxPacketSize}");
                    data = new byte[HeaderConstants.MaxPacketSize];
                    await ReceiveData(HeaderConstants.MaxPacketSize, data);
                    offset += HeaderConstants.MaxPacketSize;
                }

                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
        
        private async Task SendFile(string path)
        {
            var fileName = _fileHandler.GetFileName(path);
            var fileSize = _fileHandler.GetFileSize(path);
            var header = new Header().Create(fileName, fileSize);
            await _networkStream.WriteAsync(header, 0, header.Length);
            //_connectedSocket.Send(header, header.Length, SocketFlags.None);
            
            var fileNameToBytes = Encoding.UTF8.GetBytes(fileName);
            //_connectedSocket.Send(fileNameToBytes, fileNameToBytes.Length, SocketFlags.None);
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
                //_connectedSocket.Send(data, data.Length, SocketFlags.None);
                await _networkStream.WriteAsync(data, 0, data.Length);
                currentPart++;
            }
        }
    }
}