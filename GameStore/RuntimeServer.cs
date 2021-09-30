using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
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

        private Socket _connectedSocket;

        private readonly IGamesLogic _gamesLogic;
        private readonly IUserLogic _userLogic;
        private readonly IReviewLogic _reviewLogic;
        private readonly IFileStreamHandler _fileStreamHandler;
        private readonly IFileHandler _fileHandler;

        private User _userLogged;

        public Runtime(IServiceProvider serviceProvider)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
            _fileStreamHandler = new FileStreamHandler();
            _fileHandler = new FileHandler();
        }

        public void HandleConnection(Socket connectedSocket)
        {
            this._connectedSocket = connectedSocket;
            while (!Exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            Login(header);
                            break;
                        case CommandConstants.ListGames:
                            ListGames(header);
                            break;
                        case CommandConstants.DetailGame:
                            DetailGame(header);
                            break;
                        case CommandConstants.Purchase:
                            Purchase(header);
                            break;
                        case CommandConstants.GetReviews:
                            GetReviews(header);
                            break;
                        case CommandConstants.Publish:
                            Publish(header);
                            break;
                        case CommandConstants.Search:
                            Search(header);
                            break;
                        case CommandConstants.FilterRating:
                            FilterRating(header);
                            break;
                        case CommandConstants.ListPublishedGames:
                            ListPublishedGames(header);
                            break;
                        case CommandConstants.ModifyGame:
                            ModifyGame(header);
                            break;
                        case CommandConstants.DeleteGame:
                            DeleteGame(header);
                            break;
                        case CommandConstants.Rate:
                            Rate(header);
                            break;
                        case CommandConstants.Download:
                            Download(header);
                            break;
                        case CommandConstants.ModifyImage:
                            ModifyImage(header);
                            break;
                        case CommandConstants.ListPurchasedGames:
                            GetPurchasedGames(header);
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

        private void GetPurchasedGames(Header header)
        {
            var purchasedGames = _userLogic.GetPurchasedGames(_userLogged.Id);
            Response(purchasedGames.Count.ToString(),CommandConstants.ListPurchasedGames);
            
            SendGames(header,purchasedGames);
        }

        private void SendGames(Header header, List<Game> gamesFound)
        {
            foreach (var game in gamesFound)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                Response(gameToString, header.ICommand);
            }
        }

        private void FilterRating(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var minRating = Convert.ToInt32(Encoding.UTF8.GetString(bufferData));
            var gamesFound = _gamesLogic.GetGamesOverRating(minRating);
            Response(gamesFound.Count.ToString(), CommandConstants.FilterRating);
            
            SendGames(header, gamesFound);
        }

        private void Search(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var keywords = Encoding.UTF8.GetString(bufferData);
            var gamesFound = _gamesLogic.GetSearchedGames(keywords);
            Response(gamesFound.Count.ToString(), CommandConstants.Search);
            
            SendGames(header,gamesFound);
        }

        private void Download(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            Response($"The image of the game with id {gameId} is going to be sent", header.ICommand);
            var game = _gamesLogic.GetById(gameId);
            var path = game.Image;
            SendFile(path);
        }

        private void Rate(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);

            var splittedReview = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameId = Convert.ToInt32(splittedReview[0]);
            var reviewedGame = _gamesLogic.GetById(gameId);
            var newReview = new Review(_userLogged, reviewedGame, Convert.ToInt32(splittedReview[1]),
                splittedReview[2]);
            _reviewLogic.Add(newReview);
            _reviewLogic.AdjustRating(gameId);
            var response = $"{_userLogged.UserName} successfully reviewed {reviewedGame.Title}";
            Response(response, header.ICommand);
        }

        private void DeleteGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);

            var gameId = Convert.ToInt32(gameIdString);
            _gamesLogic.Delete(gameId);
            Response($"Your game with id {gameId} was deleted.", header.ICommand);
        }

        private void ModifyGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.Modify(modifySplit);
            Response($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }
        
        private void ModifyImage(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            ReceiveFile();
            Console.WriteLine("File received");
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.ModifyImage(modifySplit);
            Response($"Your game with id {gameModifyId} was modified.",header.ICommand);
        }

        private void ListPublishedGames(Header header)
        {
            var listPublished = _gamesLogic.GetPublishedGames(_userLogged);
            Response(listPublished.Count.ToString(),CommandConstants.ListPublishedGames);
            
            SendGames(header,listPublished);
        }

        private void Publish(Header header)
        {
            var bufferPublish = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferPublish);
            ReceiveFile();
            Console.WriteLine("File received");
            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
            var newGame = new Game(split[0], split[1], split[2], split[3])
            {
                Creator = _userLogged
            };
            var newGameInDb = _gamesLogic.Add(newGame);

            var response = $"{newGameInDb.Title} was published to the store with id {newGameInDb.Id}";
            Response(response, header.ICommand);
        }

        private void GetReviews(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var reviewsList = _reviewLogic.GetGameReviews(gameId);
            Response(reviewsList.Count.ToString(), CommandConstants.GetReviews);
            foreach (var review in reviewsList)
            {
                var reviewToString = review.Rating + "*" + review.Comment;
                Response(reviewToString, header.ICommand);
            }
        }

        private void Purchase(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var response = _userLogic.PurchaseGame(_userLogged, gameId);
            
            Response(response, header.ICommand);
        }

        private void DetailGame(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);

            var detailedGame = _gamesLogic.GetById(gameId);
            if (detailedGame != null)
            {
                var gameToString =
                    $"{detailedGame.Id}*{detailedGame.Title}*{detailedGame.Genre}*{detailedGame.Rating}*{detailedGame.Sinopsis}*{detailedGame.Image}";
                Response(gameToString, header.ICommand);
            }
            else
            {
                Response("The game selected has been deleted from the store.", header.ICommand);
            }
        }

        private void ListGames(Header header)
        {
            var list = _gamesLogic.GetAll();
            Response(list.Count.ToString(), CommandConstants.ListGames);
            
            SendGames(header,list);
        }

        private void Login(Header header)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(header.IDataLength, bufferData);
            var user = Encoding.UTF8.GetString(bufferData);
            Console.WriteLine("User: " + Encoding.UTF8.GetString(bufferData));
            var userInDb = _userLogic.Login(user);
            _userLogged = userInDb;
            var response =
                $"Log in of the user: {userInDb.UserName} (created at {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month}).";
            Response(response, header.ICommand);
        }


        private void Response(string mensaje, int command)
        {
            var header = new Header(HeaderConstants.Response, command, mensaje.Length);
            var data = header.GetRequest();
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += _connectedSocket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }

            sentBytes = 0;
            var bytesMessage = Encoding.UTF8.GetBytes(mensaje);
            while (sentBytes < bytesMessage.Length)
            {
                sentBytes += _connectedSocket.Send(bytesMessage, sentBytes, bytesMessage.Length - sentBytes,
                    SocketFlags.None);
            }
        }

        private void ReceiveData(int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = _connectedSocket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
                    if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!Exit)
                        {
                            _connectedSocket.Shutdown(SocketShutdown.Both);
                            _connectedSocket.Close();
                            throw new ClientDisconnected();
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
                    throw new ClientDisconnected();
                }
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
        
        private void SendFile(string path)
        {
            var fileName = _fileHandler.GetFileName(path);
            var fileSize = _fileHandler.GetFileSize(path);
            var header = new Header().Create(fileName, fileSize);
            _connectedSocket.Send(header, header.Length, SocketFlags.None);
            
            var fileNameToBytes = Encoding.UTF8.GetBytes(fileName);
            _connectedSocket.Send(fileNameToBytes, fileNameToBytes.Length, SocketFlags.None);
            
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
                _connectedSocket.Send(data, data.Length, SocketFlags.None);
                currentPart++;
            }
        }
    }
}