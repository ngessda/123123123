using NetCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class SimpleClient : IParser
    {
        private string host;
        private int port;
        private Socket clientSocket;

        private NetIO net;

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

                net = new NetIO(clientSocket, this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Что-то пошло не так...");
            }
        }

        public void Start()
        {
            Thread sendingThread = new Thread(Sending);
            Thread receivingThread = new Thread(() =>
            {
                net.Communicate();
                Console.WriteLine("[Info]: завершение получения сообщений");
            });

            sendingThread.Start();
            receivingThread.Start();

            try
            {
                sendingThread.Join();
                receivingThread.Join();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                net.Stop();
                Console.WriteLine("Завершение работы клиента");
            }
        }

        private void Sending()
        {
            if (clientSocket != null)
            {
                while (true)
                {
                    Console.WriteLine("Введите сообщение: ");
                    string? message = Console.ReadLine();

                    if (message == null)
                    {
                        continue;
                    }
                    message = message.Trim();

                    net.Send(message);

                    if (message.ToLower() == "quit")
                    {
                        net.Stop();
                        Console.WriteLine("[Info]: завершение отправки сообщений");
                        break;
                    }
                }
            }
        }

        public void Parse(string data)
        {
            if (data == string.Empty || data == "shutdown")
            {
                Console.WriteLine("[Info]: завершение получения сообщений");
                net.Stop();
            }
        }
    }
}