using System;
using System.Net.Sockets;
using System.Text;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;

namespace GameStoreServer
{
    public class Runtime
    {
        public bool Exit = false;
        
        private IGamesLogic _gamesLogic;
        private IUserLogic _userLogic;
        private IReviewLogic _reviewLogic;
        
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
                            var bufferData1 = new byte[header.IDataLength];  
                            ReceiveData(connectedSocket,header.IDataLength,bufferData1);
                            var user = Encoding.UTF8.GetString(bufferData1);
                            Console.WriteLine("Usuario: "+ Encoding.UTF8.GetString(bufferData1));
                            var userInDb = _userLogic.Login(user);
                            _userLogged = userInDb;
                            var response = $"Se inicio sesion en el usuario {userInDb.UserName} (creado el {userInDb.DateCreated.Day}/{userInDb.DateCreated.Month})."; 
                            Request(response, connectedSocket, CommandConstants.Login);
                            
                            break;
                        case CommandConstants.ListGames:
                            Console.WriteLine("Not Implemented yet...");
                            break;
                        case CommandConstants.Message:
                            Console.WriteLine("Will receive message to display...");
                            var bufferData = new byte[header.IDataLength];  
                            ReceiveData(connectedSocket,header.IDataLength,bufferData);
                            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server is closing, will not process more data -> Message {e.Message}..");    
                }
            }
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
                        if (!Exit)
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