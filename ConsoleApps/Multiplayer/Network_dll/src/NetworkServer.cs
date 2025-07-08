using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Network_dll.Messages.Data;
using Network.CheckSum;
using Network.FileManagement;
using Network.Messages;
using Network.Messages.Server;
using Network.Utilities;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public abstract class NetworkServer<T> : NetworkManager<T> where T : NetworkServer<T>
    {
        protected readonly List<int> clientIds = new List<int>();
        protected readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        protected readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();

        protected readonly ServerHsResponse HeldServerHsSa;

        public Action<int> onNewClient;
        public Action<int> onClientRemoved;

        protected int nextClientId = 0; // This id should be generated during first handshake
        protected SavedClientHandler _savedClientHandler = new SavedClientHandler("SavedClients.txt");

        public int targetFPS = 60;
        public int afkDisconnectTime = 12500;
        private bool shouldStop = false;

        public void StartServer(int port)
        {
            Initialize();
            this.port = port;
            ipAddress = GetIp();
            connection = new UdpConnection(port, this);
            connection.FlushReceiveData();
            rngGenerator = new Random((int)ServerTime.time);
            seed = rngGenerator.Next(0, int.MaxValue);
            rngGenerator = new Random(seed);
            Console.WriteLine($"Server initialized on port {port}");
            Logger.Log($"Server Seed: {seed}");
            OperationsList.Populate(rngGenerator);
        }

        public void ServerLoop()
        {
            while (!shouldStop)
            {
                Update();
                Thread.Sleep(1000 / targetFPS);
            }
        }

        public override void Update()
        {
            base.Update();
            PingCheck();
        }

        public void EndServer()
        {
            if (connection == null)
                return;

            shouldStop = true;
            Broadcast(new Disconnect(-1).Serialize());
            connection = null;
        }

        protected void AddClient(IPEndPoint ip, string username)
        {
            //ipToId does not contain a previous connected Ip? Has something to do with creating a new Connection? 
            if (!ipToId.ContainsKey(ip))
            {
                int id = nextClientId;
                ipToId[ip] = nextClientId;
                Logger.Log("Adding client: " + ip.Address + " ID: " + id);
                clients.Add(nextClientId, new Client(ip, nextClientId, username, ServerTime.time));
                clients[nextClientId].lastPingTime = ServerTime.time;
                clientIds.Add(id);

                idToIVKeyGenerator[id] = new Random(seed);

                SendToClient(new Ping(0).Serialize(), ipToId[ip]);
                onNewClient?.Invoke(nextClientId);
                nextClientId++;
            }
        }

        protected void RemoveClient(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
            {
                Logger.Log("Removing client: " + ip.Address);
                idToIVKeyGenerator.Remove(ipToId[ip]);
                clients.Remove(ipToId[ip]);
                clientIds.Remove(ipToId[ip]);
                onClientRemoved?.Invoke(ipToId[ip]);
                ipToId.Remove(ip);
            }
        }

        protected virtual void RemoveClient(int id)
        {
            if (ipToId.ContainsValue(id))
            {
                Logger.Log("Removing client: " + id);
                ipToId.Remove(clients[id].ipEndPoint);
                clientIdToMessageId.Remove(id);
                idToIVKeyGenerator.Remove(id);
                clients.Remove(id);
                clientIds.Remove(id);
                onClientRemoved?.Invoke(id);
            }
        }

        public void SendToClient(byte[] data, int clientId)
        {
            Client client = clients[clientId];
            if (client == null)
                Logger.LogError("Client Was Null");

            if (connection != null)
                connection.Send(data, client.ipEndPoint);
        }

        public void SendToIp(byte[] data, IPEndPoint ip)
        {
            if (connection != null)
                connection.Send(data, ip);
        }

        public void Broadcast(byte[] data)
        {
            if (connection == null)
                return;

            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    connection.Send(data, iterator.Current.Value.ipEndPoint);
                }
            }
        }

        public int GetReceivedClientId(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
                return ipToId[ip];

            return nextClientId;
        }

        private void PingCheck()
        {
            List<ClientPing> clientPings = new List<ClientPing>();

            for (int i = 0; i < clientIds.Count; i++)
            {
                short ms = (short)((ServerTime.time - clients[clientIds[i]].lastPingTime) * 1000);
                ClientPing ping;
                ping.id = clientIds[i];
                ping.ms = ms;
                clientPings.Add(ping);

                if (ms > TimeOutTime * 1000)
                {
                    Broadcast(new Disconnect(clientIds[i]).Serialize());
                    RemoveClient(clientIds[i]);
                }
            }

            Broadcast(new AllPings(clientPings.ToArray(), clientIds.Count).Serialize());
            clientPings.Clear();
        }

        private IPAddress GetIp()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address;
        }
    }
}