﻿using System;
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
        public bool Exit { get; set; }
        
        private IGamesLogic _gamesLogic;
        private IUserLogic _userLogic;
        private IReviewLogic _reviewLogic;

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
                            Response(response, connectedSocket, CommandConstants.Login);

                            break;
                        case CommandConstants.ListGames:
                            var list = _gamesLogic.GetAll();
                            Response(list.Count.ToString(), connectedSocket, CommandConstants.ListGames);
                            for (int i = 0; i < list.Count; i++)
                            {
                                var game = list[i];
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, CommandConstants.ListGames);
                            }

                            break;
                        case CommandConstants.Purchase:
                            //Agarra userLogged y le mete el game por medio del id que le pase en el mensaje de la request. Si ya lo tiene le avisa que no se le agrego.
                            var bufferPurchase = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferPurchase);
                            gameIdString = Encoding.UTF8.GetString(bufferPurchase);

                            gameId = Convert.ToInt32(gameIdString);
                            var purchased = _userLogic.PurchaseGame(userLogged, gameId);
                            if (purchased)
                            {
                                response = $"Game {gameId} was purchased by {userLogged.UserName}";
                            }
                            else
                            {
                                response = $"The game {gameId} is already purchased by {userLogged.UserName}";
                            }

                            Response(response, connectedSocket, CommandConstants.Login);

                            break;
                        case CommandConstants.Publish:
                            var bufferPublish = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferPublish);

                            var split = (Encoding.UTF8.GetString(bufferPublish)).Split("*");
                            var newGame = new Game(split[0], split[1], split[2]);
                            var newGameInDb = _gamesLogic.Add(newGame);
                            _userLogic.NewGame(newGameInDb, userLogged);
                            response = $"El juego {newGame.Title} fue añadido al store";
                            Response(response, connectedSocket,CommandConstants.Publish);
                            break;
                        case CommandConstants.Search:
                            var bufferSearch = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferSearch);
                            var keywords = Encoding.UTF8.GetString(bufferSearch);
                            var foundedGames = _gamesLogic.GetSearchedGames(keywords);
                            Response(foundedGames.Count.ToString(), connectedSocket, CommandConstants.Search);
                            for (int i = 0; i < foundedGames.Count; i++)
                            {
                                var game = foundedGames[i];
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, CommandConstants.Search);
                            }

                            break;
                        case CommandConstants.ListPublishedGames:
                            var listPublished = userLogged.PublishedGames;
                            Response(listPublished.Count.ToString(), connectedSocket, CommandConstants.ListPublishedGames);
                            for (int i = 0; i < listPublished.Count; i++)
                            {
                                var game = listPublished[i];
                                string gameToString =
                                    $"{game.Id}*{game.Title}*{game.Genre}*{game.Rating}*{game.Sinopsis}*{game.Image}";
                                Response(gameToString, connectedSocket, CommandConstants.ListGames);
                            }
                            break;
                        case CommandConstants.DeleteGame:
                            var bufferDelete = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferDelete);
                            gameIdString = Encoding.UTF8.GetString(bufferDelete);

                            gameId = Convert.ToInt32(gameIdString);
                            _gamesLogic.Delete(gameId);
                            Response($"Your game with id {gameId} was deleted.", connectedSocket, CommandConstants.DeleteGame);
                            break;
                        case CommandConstants.Message:
                            Console.WriteLine("Will receive message to display...");
                            var bufferData = new byte[header.IDataLength];
                            ReceiveData(connectedSocket, header.IDataLength, bufferData);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                            break;
                    }
                }
                catch (ClientDisconnected c)
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