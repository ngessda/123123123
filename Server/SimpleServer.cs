using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private bool flag = false;
        private Socket clientSocket;

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
                while(true)
                {
                    Console.WriteLine("Ожидание нового подключения");
                    clientSocket = serverSocket.Accept();
                    Console.WriteLine("Соединение с клиентом установлена");
                    t = new Thread(ThreadOnServer);
                    t.Start();
                    if (flag)
                    {
                        Console.WriteLine("Была получена команда stop - сервер завершает свою работу");
                        break;
                    }
                }
            }
            Console.WriteLine("Сервер завершил свою работу");
        }

        private void ThreadOnServer()
        {
            var request = ReceiveData(clientSocket);
            Console.WriteLine("Получено сообщение от клиента: {0}", request);

            string response = "Получено " + request.Length.ToString() + " символов";
            SendData(clientSocket, response);
            if (request.Trim().ToLower() == "quit")
            {
                clientSocket.Close();
            }

            if (request.Trim().ToLower() == "stop")
            {
                flag = true;
            }
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
