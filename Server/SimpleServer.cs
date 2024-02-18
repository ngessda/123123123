using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class SimpleServer
    {
        private Thread t = null;
        private object locker = new object();
        private int port = 1488;
        private string host = "localhost";
        private Socket serverSocket;
        private int id = 1;

        private Dictionary<int, Socket> clients = new Dictionary<int, Socket>();

        public SimpleServer()
        {
            try
            {
                serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                foreach (var addr in Dns.GetHostEntry(host).AddressList)
                {
                    try
                    {
                        serverSocket.Bind(
                            new IPEndPoint(addr, port)
                            );
                        Console.WriteLine("Сокет связан с {0}:{1}", addr, port);
                        break;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Не удалось связать сокет с {0}:{1}", addr, port);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка в работе сокета");
            }
        }
        
        public void Start()
        {
            if(serverSocket != null)
            {
                serverSocket.Listen(10);
                Console.WriteLine("Началось прослушивание");

                Thread acceptingThread = new Thread(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("Ожидание нового подключения");
                        var clientSocket = serverSocket.Accept();
                        Console.WriteLine("Соединение с клиентом установлена");

                        lock (clients)
                        {
                            clients.Add(id, clientSocket);
                            id++;
                        }

                        Communicate(clientSocket, id);
                    }
                });

                acceptingThread.Start();
            }
            Console.WriteLine("Сервер завершил свою работу");
        }

        private void Communicate(Socket clientSocket, int _id)
        {
            Thread servingThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        string message = ReceiveData(clientSocket);

                        if (message == string.Empty)
                        {
                            lock (clients)
                            {
                                clients.Remove(_id);
                            }
                            clientSocket.Close();
                            Console.WriteLine("[Client]: отключен");
                            break;
                        }
                        message = message.Trim();
                        Console.WriteLine("[Client]: {0}", message);

                        if (message == "quit")
                        {
                            lock (clients)
                            {
                                clients.Remove(_id);
                            }
                            SendData(clientSocket, "shutdown");
                            clientSocket.Close();
                            Console.WriteLine("[Client]: отключен");
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("[Error]: не удалось получить данные");
                        break;
                    }

                }
            });
            servingThread.Start();
        }

        private string ReceiveData(Socket clientSocket)
        {
            var buffer = new byte[1024];
            var count = clientSocket.Receive(buffer);
            Console.WriteLine("Получено {0} байтов", count);
            var result = Encoding.UTF8.GetString(buffer, 0, count);
            return result;
        }

        private void SendData(Socket clientSocket, String data)
        {
            byte[] replyBuffer = Encoding.UTF8.GetBytes(data);
            clientSocket.Send(replyBuffer);
        }
    }
}
