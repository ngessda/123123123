using NetCommunication;
using System.Net.Sockets;

namespace Server
{
    public class ConnectedClient : IParser
    {
        private NetIO net;
        private static Dictionary<int, ConnectedClient> _clients = new Dictionary<int, ConnectedClient>();
        private int _id;

<<<<<<< HEAD
        public string? Username { get; private set; } = null;

=======
>>>>>>> fbbb3548d649a15bd7ae573b1615240b931cbabe
        public ConnectedClient(Socket socket, int id)
        {
            net = new NetIO(socket, this);
            _id = id;
        }

        public void Parse(string data)
        {
            if (data == string.Empty)
            {
<<<<<<< HEAD
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
=======
                lock (_clients)
                {
                    _clients.Remove(_id);
                }
                net.Stop();
                Console.WriteLine("[Clients #{0}]: отключен", _id);
            }

            var message = data.Trim();
            Console.WriteLine("[Clients #{0}]: {1}", _id, message);

            if (message == "quit")
            {
                lock(_clients)
                {
                    _clients.Remove(_id);
                }
                net.Send("shutdown");
                net.Stop();
                Console.WriteLine("[Clients #{0}]: отключен", _id);
>>>>>>> fbbb3548d649a15bd7ae573b1615240b931cbabe
            }
        }

        public void Start()
        {
            var communication = new Thread(() =>
            {
<<<<<<< HEAD
=======
                lock (_clients)
                {
                    _clients.Add(_id, this);
                }
>>>>>>> fbbb3548d649a15bd7ae573b1615240b931cbabe
                net.Communicate();
            });

            communication.Start();
        }
    }
}
