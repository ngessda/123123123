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
        private int port = 80;
        private string host = "localhost";
        private Socket serverSocket;

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
                    catch
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
                serverSocket.Listen(port);
                Console.WriteLine("Началось прослушивание");
                while(true)
                {
                    Console.WriteLine("Ожидание нового подключения");
                    var clientSocket = serverSocket.Accept();
                    Console.WriteLine("Соединение с клиентом установлена");

                    var request = ReceiveData(clientSocket);
                    Console.WriteLine("Получено сообщение от клиента: {0}", request);

                    string response = "Получено " + request.Length.ToString() + " символов";

                    clientSocket.Close();

                    if (request.Trim().ToLower() == "stop")
                    {
                        Console.WriteLine("Была получена команда stop - сервер завершает свою работу");
                        break;
                    }
                }
            }
            Console.WriteLine("Сервер завершил свою работу");
        }
        public string ReceiveData(Socket clientSocket)
        {
            var buffer = new byte[1024];
            var count = clientSocket.Receive(buffer);
            Console.WriteLine("Получено {0} байтов", count);
            var result = Encoding.UTF8.GetString(buffer, 0, count);
            return result;
        }

        public void SendData(Socket clientSocket, String data)
        {
            byte[] replyBuffer = Encoding.UTF8.GetBytes(data);
            clientSocket.Send(replyBuffer);
        }
    }
}
