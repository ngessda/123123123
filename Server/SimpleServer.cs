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

                        new ConnectedClient(clientSocket, id).Start();
                    }
                });

                acceptingThread.Start();
            }
            Console.WriteLine("Сервер завершил свою работу");
        }
    }
}
