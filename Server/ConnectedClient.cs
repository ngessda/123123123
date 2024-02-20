using NetCommunication;
using System.Net.Sockets;

namespace Server
{
    public class ConnectedClient : IParser
    {
        private NetIO net;
        private static Dictionary<int, ConnectedClient> _clients = new Dictionary<int, ConnectedClient>();
        private int _id;

        public ConnectedClient(Socket socket, int id)
        {
            net = new NetIO(socket, this);
            _id = id;
        }

        public void Parse(string data)
        {
            if (data == string.Empty)
            {
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
            }
        }

        public void Start()
        {
            var communication = new Thread(() =>
            {
                lock (_clients)
                {
                    _clients.Add(_id, this);
                }
                net.Communicate();
            });

            communication.Start();
        }
    }
}
