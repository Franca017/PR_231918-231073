using System;
using System.Net.Sockets;
using System.Text;
using Domain;
using Domain.Exceptions;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;

namespace GameStoreServer
{
    public class Runtime
    {
        private bool Exit { get; set; }
        
        private readonly IGamesLogic _gamesLogic;
        private readonly IUserLogic _userLogic;
        private readonly IReviewLogic _reviewLogic;

        private User _userLogged;

        public Runtime(IServiceProvider serviceProvider)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
        }

        public void HandleConnection(Socket connectedSocket)
        {
            while (!Exit)
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
                            Login(header, connectedSocket);
                            break;
                        case CommandConstants.ListGames:
                            ListGames(header, connectedSocket);
                            break;
                        case CommandConstants.DetailGame:
                            DetailGame(header, connectedSocket);
                            break;
                        case CommandConstants.Purchase:
                            Purchase(header, connectedSocket);
                            break;
                        case CommandConstants.GetReviews:
                            GetReviews(header, connectedSocket);
                            break;
                        case CommandConstants.Publish:
                            Publish(header, connectedSocket);
                            break;
                        case CommandConstants.Search:
                            Search(header, connectedSocket);
                            break;
                        case CommandConstants.FilterRating:
                            FilterRating(header, connectedSocket);
                            break;
                        case CommandConstants.ListPublishedGames:
                            ListPublishedGames(header, connectedSocket);
                            break;
                        case CommandConstants.ModifyGame:
                            ModifyGame(header, connectedSocket);
                            break;
                        case CommandConstants.DeleteGame:
                            DeleteGame(header, connectedSocket);
                            break;
                        case CommandConstants.Rate:
                            Rate(header, connectedSocket);
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
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
        }

        private void FilterRating(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var minRating = Convert.ToInt32(Encoding.UTF8.GetString(bufferData));
            var gamesFound = _gamesLogic.GetGamesOverRating(minRating);
            Response(gamesFound.Count.ToString(), connectedSocket, CommandConstants.FilterRating);
            foreach (var game in gamesFound)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                Response(gameToString, connectedSocket, header.ICommand);
            }
        }
        
        private void Search(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var keywords = Encoding.UTF8.GetString(bufferData);
            var gamesFound = _gamesLogic.GetSearchedGames(keywords);
            Response(gamesFound.Count.ToString(), connectedSocket, CommandConstants.Search);
            foreach (var game in gamesFound)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                Response(gameToString, connectedSocket, header.ICommand);
            }
        }

        private void Rate(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);

            var splittedReview = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameId = Convert.ToInt32(splittedReview[0]);
            var reviewedGame = _gamesLogic.GetById(gameId);
            var newReview = new Review(_userLogged, reviewedGame, Convert.ToInt32(splittedReview[1]),
                splittedReview[2]);
            _reviewLogic.Add(newReview);
            _reviewLogic.AdjustRating(gameId);
            var response = $"{_userLogged.UserName} successfully reviewed {reviewedGame.Title}";
            Response(response, connectedSocket, header.ICommand);
        }

        private void DeleteGame(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);

            var gameId = Convert.ToInt32(gameIdString);
            _gamesLogic.Delete(gameId);
            Response($"Your game with id {gameId} was deleted.", connectedSocket, header.ICommand);
        }

        private void ModifyGame(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var modifySplit = (Encoding.UTF8.GetString(bufferData)).Split("*");
            var gameModifyId = Convert.ToInt32(modifySplit[0]);
            _gamesLogic.Modify(modifySplit);
            Response($"Your game with id {gameModifyId} was modified.", connectedSocket,
                header.ICommand);
        }

        private void ListPublishedGames(Header header, Socket connectedSocket)
        {
            var listPublished = _gamesLogic.GetPublishedGames(_userLogged);
            Response(listPublished.Count.ToString(), connectedSocket,
                CommandConstants.ListPublishedGames);
            foreach (var game in listPublished)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                Response(gameToString, connectedSocket, header.ICommand);
            }
        }

        private void Publish(Header header, Socket connectedSocket)
        {
            var bufferPublish = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferPublish);

            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
            var newGame = new Game(split[0], split[1], split[2])
            {
                Creator = _userLogged
            };
            var newGameInDb = _gamesLogic.Add(newGame);

            var response = $"{newGameInDb.Title} was published to the store with id {newGameInDb.Id}";
            Response(response, connectedSocket, header.ICommand);
        }

        private void GetReviews(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var reviewsList = _reviewLogic.GetGameReviews(gameId);
            Response(reviewsList.Count.ToString(), connectedSocket, CommandConstants.GetReviews);
            foreach (var review in reviewsList)
            {
                var reviewToString = review.Rating + "*" + review.Comment;
                Response(reviewToString, connectedSocket, header.ICommand);
            }
        }

        private void Purchase(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);
            var response = _userLogic.PurchaseGame(_userLogged, gameId);
            
            Response(response, connectedSocket, header.ICommand);
        }

        private void DetailGame(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var gameIdString = Encoding.UTF8.GetString(bufferData);
            var gameId = Convert.ToInt32(gameIdString);

            var detailedGame = _gamesLogic.GetById(gameId);
            if (detailedGame != null)
            {
                var gameToString =
                    $"{detailedGame.Id}*{detailedGame.Title}*{detailedGame.Genre}*{detailedGame.Rating}*{detailedGame.Sinopsis}*{detailedGame.Image}";
                Response(gameToString, connectedSocket, header.ICommand);
            }
            else
            {
                Response("The game selected has been deleted from the store.", connectedSocket, header.ICommand);
            }
        }

        private void ListGames(Header header, Socket connectedSocket)
        {
            var list = _gamesLogic.GetAll();
            Response(list.Count.ToString(), connectedSocket, CommandConstants.ListGames);
            foreach (var game in list)
            {
                var gameToString =
                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                Response(gameToString, connectedSocket, header.ICommand);
            }
        }

        private void Login(Header header, Socket connectedSocket)
        {
            var bufferData = new byte[header.IDataLength];
            ReceiveData(connectedSocket, header.IDataLength, bufferData);
            var user = Encoding.UTF8.GetString(bufferData);
            Console.WriteLine("Usuario: " + Encoding.UTF8.GetString(bufferData));
            var userInDb = _userLogic.Login(user);
            _userLogged = userInDb;
            var response =
                $"Se inicio sesion en el usuario {userInDb.UserName} (creado el {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month}).";
            Response(response, connectedSocket, header.ICommand);
        }


        private void Response(string mensaje, Socket socket, int command)
        {
            var header = new Header(HeaderConstants.Response, command, mensaje.Length);
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
                        if (!Exit)
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
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
    }
}