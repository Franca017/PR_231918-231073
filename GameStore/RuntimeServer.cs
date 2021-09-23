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

        public Runtime(IServiceProvider serviceProvider)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
        }

        public void HandleConnection(Socket connectedSocket)
        {
            User userLogged = null;
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
                    string gameIdString;
                    int gameId;
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            var bufferData1 = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferData1);
                            var user = Encoding.UTF8.GetString(bufferData1);
                            Console.WriteLine("Usuario: " + Encoding.UTF8.GetString(bufferData1));
                            var userInDb = _userLogic.Login(user);
                            userLogged = userInDb;
                            var response =
                                $"Se inicio sesion en el usuario {userInDb.UserName} (creado el {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month}).";
                            Response(response, connectedSocket, header.ICommand);

                            break;
                        case CommandConstants.ListGames:
                            var list = _gamesLogic.GetAll();
                            Response(list.Count.ToString(), connectedSocket, CommandConstants.ListGames);
                            foreach (var game in list)
                            {
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, header.ICommand);
                            }

                            break;
                        case CommandConstants.Purchase:
                            //Agarra userLogged y le mete el game por medio del id que le pase en el mensaje de la request. Si ya lo tiene le avisa que no se le agrego.
                            var bufferPurchase = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferPurchase);
                            gameIdString = Encoding.UTF8.GetString(bufferPurchase);

                            gameId = Convert.ToInt32(gameIdString);
                            var purchased = _userLogic.PurchaseGame(userLogged, gameId);
                            response = purchased ? $"Game {gameId} was purchased by {userLogged.UserName}" 
                                : $"The game {gameId} is already purchased by {userLogged.UserName}";

                            Response(response, connectedSocket, header.ICommand);

                            break;
                        case CommandConstants.Publish:
                            var bufferPublish = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferPublish);

                            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
                            var newGame = new Game(split[0], split[1], split[2])
                            {
                                Creator = userLogged
                            };
                            var newGameInDb = _gamesLogic.Add(newGame);
                            
                            response = $"{newGameInDb.Title} was published to the store with id {newGameInDb.Id}";
                            Response(response, connectedSocket, header.ICommand);
                            break;
                        case CommandConstants.Search:
                            var bufferSearch = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferSearch);
                            var keywords = Encoding.UTF8.GetString(bufferSearch);
                            var foundedGames = _gamesLogic.GetSearchedGames(keywords);
                            Response(foundedGames.Count.ToString(), connectedSocket, CommandConstants.Search);
                            foreach (var game in foundedGames)
                            {
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, header.ICommand);
                            }

                            break;
                        case CommandConstants.ListPublishedGames:
                            var listPublished = _gamesLogic.GetPublishedGames(userLogged);
                            Response(listPublished.Count.ToString(), connectedSocket, CommandConstants.ListPublishedGames);
                            foreach (var game in listPublished)
                            {
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, header.ICommand);
                            }
                            break;
                        case CommandConstants.DeleteGame:
                            var bufferDelete = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferDelete);
                            gameIdString = Encoding.UTF8.GetString(bufferDelete);

                            gameId = Convert.ToInt32(gameIdString);
                            _gamesLogic.Delete(gameId);
                            Response($"Your game with id {gameId} was deleted.", connectedSocket, header.ICommand);
                            break;
                        case CommandConstants.Message:
                            Console.WriteLine("Will receive message to display...");
                            var bufferData = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferData);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                            break;
                    }
                }
                catch (ClientDisconnected)
                {
                    Console.WriteLine($"{userLogged.UserName} disconnected");
                    Exit = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
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