using NetCommunication;
using System.Net.Sockets;

namespace Client
{
    public class SimpleClient : IParser
    {
        private string host;
        private int port;
        private Socket clientSocket;
        private bool _correctUsername = false;
        private bool CorrectUsername
        {
            get
            {
                lock (_lock)
                {
                    return _correctUsername;
                }
            }
            set
            {
                lock (_lock)
                {
                    _correctUsername = value;
                }
            }
        }

        private NetIO net;

        private object _lock = new object();
        private bool _isLogged = false;
        private bool IsLogged
        {
            get
            {
                lock (_lock)
                {
                    return _isLogged;
                }
            }
            set
            {
                lock (_lock)
                {
                    _isLogged = value;
                }
            }
        }


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
                    if (!IsLogged)
                    {
                        Console.Write("Введите свой ник: ");
                        string? data = Console.ReadLine();
                        if (data == null)
                        {
                            continue;
                        }
                        data = data.Trim();
                        net.Send($"LOGIN={data}=END");
                        lock (_lock)
                        {
                            Monitor.Wait(_lock);
                        }
                        continue;
                    }
                    Console.Write("Введите сообщение: ");
                    string? message = Console.ReadLine();

                    if (message == null)
                    {
                        continue;
                    }
                    message = message.Trim();

                    if (message.ToLower() == "quit")
                    {
                        net.Send("QUIT= =END");
                        net.Stop();
                        Console.WriteLine("[Info]: завершение отправки сообщений");
                        break;
                    }
                    else if(message.ToLower() == "private")
                    {
                        Console.Write("Введите имя пользователя: ");
                        string? tempUsername = Console.ReadLine();
                        net.Send($"PRIVATE={tempUsername}=END");
                        Console.Write("Введите сообщение пользователю: ");
                        string? tempMessage = Console.ReadLine();
                        net.Send($"PRIVATE={tempMessage}=END");
                    }
                    else
                    {
                        net.Send($"MESSAGE={message}=END");
                    }
                }
            }
        }
        

        public void Parse(string data)
        {
            if (data == string.Empty)
            {
                Console.WriteLine("[Info]: завершение получения сообщений");
                if (!IsLogged)
                {
                    Console.WriteLine("[Info]: сервер не отвечает - пишите quit");
                    IsLogged = true;
                    lock (_lock)
                    {
                        Monitor.Pulse(_lock);
                    }
                }
                net.Stop();
            }

            var splitted = data.Split('=');
            var command = splitted[0];
            var inner = splitted[1];

            switch (command)
            {
                case "LOGIN":
                    var loginData = inner.Split(':');
                    var result = loginData[0];
                    var reason = loginData[1];
                    if (result == "failed")
                    {
                        Console.WriteLine("[Error]: {0}", reason);
                        IsLogged = false;
                    }
                    else
                    {
                        IsLogged = true;
                        Console.WriteLine("[Info]: {0}", reason);
                    }
                    lock (_lock)
                    {
                        Monitor.Pulse(_lock);
                    }
                    break;

                case "BROADCAST":
                    Console.WriteLine("\n" + inner);
                    break;

                case "PRIVATE":
                    Console.WriteLine("\n" + inner);
                    break;

                default:
                    Console.WriteLine("[Error]: неизвестная команда");
                    break;
            }
        }
    }
}