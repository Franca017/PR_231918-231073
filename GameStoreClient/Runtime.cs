﻿using System;
using System.Net.Sockets;
using System.Text;
using Domain;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary;

namespace GameStoreClient
{
    public class Runtime
    {
        private IGamesLogic _gamesLogic;
        private IUserLogic _userLogic;
        private IReviewLogic _reviewLogic;
        
        static bool _exit = false;
        public Runtime(IServiceProvider serviceProvider)
        {
            _gamesLogic = serviceProvider.GetService<IGamesLogic>();
            _userLogic = serviceProvider.GetService<IUserLogic>();
            _reviewLogic = serviceProvider.GetService<IReviewLogic>();
        }

        public void Execute(Socket socket)
        {
            bool logged = false;
            while (!logged)
            {
                Console.WriteLine("Bienvenido al Sistema Client");
                Console.Write("Ingrese su nombre de Usuario (en caso de no existir se le creara uno): ");
                var usuario = Console.ReadLine();
                Request(usuario, socket, CommandConstants.Login);
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(socket, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    var bufferData1 = new byte[header.IDataLength];
                    ReceiveData(socket, header.IDataLength, bufferData1);
                    Console.WriteLine(Encoding.UTF8.GetString(bufferData1));

                    logged = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"---- -> Message {e.Message}..");
                }
            }

            while (!_exit)
            {
                Console.WriteLine("Opciones validas: ");
                Console.WriteLine("message -> envia un mensaje al server");
                Console.WriteLine("exit -> abandonar el programa");
                Console.Write("Ingrese su opcion: ");
                var opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "exit":
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        _exit = true;
                        break;
                    case "message":
                        Console.WriteLine("Ingrese el mensaje a enviar:");
                        var mensaje = Console.ReadLine();
                        Request(mensaje, socket, CommandConstants.Message);

                        break;
                    default:
                        Console.WriteLine("Opcion invalida");
                        break;
                }
            }

            Console.WriteLine("Exiting Application");
        }
        
        private static void Request(string mensaje, Socket socket, int command)
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
        
        private static void ReceiveData(Socket clientSocket,  int length, byte[] buffer)
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