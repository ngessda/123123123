using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class SimpleClient
    {
        private string host;
        private int port;
        private Socket clientSocket;
        public bool ClientFlag { get; private set; } = false;

        public SimpleClient(string serverHost, int serverPort)
        {
            try
            {
                host = serverHost;
                port = serverPort;
                Console.WriteLine("Подключение к {0}:{1}", host, port);
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(host, port);
                Console.WriteLine("Соединение установлено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Что-то пошло не так...");
            }
        }

        public void Start()
        {
            while (true)
            {
                clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(host, port);
                Console.Write("Введите сообщение: ");
                string? message = Console.ReadLine();
                if (message != null && clientSocket != null && message.Trim().ToLower() != "quit")
                {
                    SendData(clientSocket, message);
                    var result = ReceiveData(clientSocket);
                    Console.WriteLine("Получено сообщение от сервера: {0}", result);
                }
                else
                {
                    ClientFlag = true;
                    clientSocket.Close();
                    Console.WriteLine("Соединение закрыто");
                    Console.WriteLine("Клиент завершил свою работу");
                    break;
                }
            }
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