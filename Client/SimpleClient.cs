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
            Thread sendingThread = new Thread(Sending);
            Thread receivingThread = new Thread(Receiving);

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
                clientSocket.Close();
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
                    try
                    {
                        SendData(clientSocket, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Error]: не удалось отправить сообщение");
                    }

                    if (message.ToLower() == "quit")
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        Console.WriteLine("[Info]: завершение отправки сообщений");
                        break;
                    }
                }
            }
        }

        private void Receiving()
        {
            if(clientSocket != null)
            {
                while (true)
                {
                    if (!clientSocket.Connected)
                    {
                        Console.WriteLine("[Info]: завершение получения сообщений");
                        break;
                    }
                    try
                    {
                        string response = ReceiveData(clientSocket);

                        if (response == string.Empty)
                        {
                            Console.WriteLine("[Info]: завершение получения сообщений");
                            break;
                        }

                        Console.WriteLine("[Server]: {0}", response);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Error]: ошибка при чтении данных, завершение чтения");
                        break;
                    }
                }
            }
        }

        public string ReceiveData(Socket clientSocket)
        {
            var buffer = new byte[1024];
            var count = clientSocket.Receive(buffer);
            if (count == 0)
            {
                return string.Empty;
            }
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