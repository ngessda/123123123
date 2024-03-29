﻿using NetCommunication;
using System.Net.Sockets;

namespace Server
{
    public class ConnectedClient : IParser
    {
        private NetIO net;
        private static Dictionary<int, ConnectedClient> _clients = new Dictionary<int, ConnectedClient>();
        private int _id;
        private bool _currentUsername = false;

        public string? Username { get; private set; } = null;

        public ConnectedClient(Socket socket, int id)
        {
            net = new NetIO(socket, this);
            _id = id;
        }

        public void Parse(string data)
        {
            string? tempUsername = null;
            string? tempMessage = null;
            bool flag = false;

            if (data == string.Empty)
            {
                if (Username != null)
                {
                    lock (_clients)
                    {
                        _clients.Remove(_id);
                    }
                    Console.WriteLine("[Clients #{0}]: отключен", _id);
                }
                net.Stop();
            }

            data = data.Trim();
            if (Username == null)
            {
                Console.WriteLine("[Unknown]: {0}", data);
            }
            else
            {
                Console.WriteLine("[{0}]: {1}", Username, data);
            }

            var splitted = data.Split('=');

            var command = splitted[0];

            var inner = splitted[1];
            switch (command)
            {
                case "LOGIN":
                    lock (_clients)
                    {
                        foreach (var client in _clients)
                        {
                            if(client.Value.Username == inner)
                            {
                                net.Send("LOGIN=failed:Такой пользователь уже существует=END");
                                return;
                            }
                        }
                        Username = inner;
                        _clients.Add(this._id, this);
                        SendAllJoinedUser($"{Username} подключился к чату.", this);
                        net.Send($"LOGIN=success:Добро пожаловать, {Username}!=END");
                    }
                    break;

                case "QUIT":
                    lock (_clients)
                    {
                        _clients.Remove(this._id);
                    }
                    net.Stop();
                    Console.WriteLine("[{0}]: отключен", Username);
                    SendAllUserLeaved($"{Username} отключился от чата.", this);
                    break;

                case "MESSAGE":
                    Console.WriteLine("[{0}]: {1}", Username, inner);

                    SendAllExcept($"[{Username}]: {inner}", this);
                    break;

                case "PRIVATE":

                    var secondInner = splitted[2];

                    foreach (var client in _clients)
                    {
                        if (client.Value.Username == inner)
                        {
                            tempUsername = inner;
                            flag = true;
                            break;
                        }

                    }
                    if(flag)
                    {
                        tempMessage = secondInner;
                        var key = _clients.FirstOrDefault(client => client.Value.Username == tempUsername).Key;
                        _clients[key].net.Send($"PRIVATE=private [{Username}]: {tempMessage}=END");
                        net.Send("PRIVATE=[Info]: Сообщение успешно отправлено!=END");
                        flag = false;
                    }
                    else
                    {
                        net.Send("PRIVATE=[Error]: Такого пользователя не существует=END");
                    }
                    break;

                default:
                    Console.WriteLine("[Error]: неизвестная команда");
                    if (Username == null)
                    {
                        net.Stop();
                    }
                    break;
            }
        }
        private static void SendAllExcept(string message, ConnectedClient? exception = null)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client.Value != exception)
                    {
                        client.Value.net.Send($"BROADCAST={message}=END");
                    }
                }
            }
        }

        private static void SendAllJoinedUser(string message, ConnectedClient? exception = null)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client.Value != exception)
                    {
                        client.Value.net.Send($"BROADCAST={message}=END");
                    }
                }
            }
        }

        private static void SendAllUserLeaved(string message, ConnectedClient? exception = null)
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    if (client.Value != exception)
                    {
                        client.Value.net.Send($"BROADCAST={message}=END");
                    }
                }
            }
        }



        public void Start()
        {
            var communication = new Thread(() =>
            {
                net.Communicate();
            });

            communication.Start();
        }
    }
}
