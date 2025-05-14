using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Network.CheckSum;
using Network.Messages;
using Network.Messages.Server;
using UnityEngine;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public class Client
    {
        public float timeStamp;
        public int id;
        public IPEndPoint ipEndPoint;
        public short ping = 0;

        public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
        {
            this.timeStamp = timeStamp;
            this.id = id;
            this.ipEndPoint = ipEndPoint;
            ping = 0;
        }
    }

    public abstract class NetworkServer<T> : NetworkManager<T> where T : NetworkServer<T>
    {
        protected readonly List<int> clientIds = new List<int>();
        protected readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        protected readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
        protected readonly Dictionary<int, float> idLastPingTime = new Dictionary<int, float>();

        protected readonly ServerHsResponse HeldServerHsSa;

        public Action<int> onNewClient;
        public Action<int> onClientRemoved;

        protected int nextClientId = 0; // This id should be generated during first handshake

        public void StartServer()
        {
            port = defaultPort;
            ipAddress = GetIp();
            connection = new UdpConnection(port, this);
            rngGenerator = new Random((int)Time.realtimeSinceStartup);
            seed = rngGenerator.Next(0, int.MaxValue);
            rngGenerator = new Random(seed);
            Debug.Log($"Server Seed: {seed}");

            OperationsList.Populate(rngGenerator);
        }


        protected override void Update()
        {
            base.Update();
            PingCheck();
        }

        private void OnDestroy()
        {
            EndServer();
        }

        public void EndServer()
        {
            if (connection == null)
                return;

            Broadcast(new Disconnect(-1).Serialize());
            connection = null;
            Instance = null;
            Destroy(gameObject);
        }

        protected void AddClient(IPEndPoint ip)
        {
            //ipToId does not contain a previous connected Ip? Has something to do with creating a new Connection? 
            if (!ipToId.ContainsKey(ip))
            {
                int id = nextClientId;
                ipToId[ip] = nextClientId;
                Debug.Log("Adding client: " + ip.Address + " ID: " + id);
                clients.Add(nextClientId, new Client(ip, nextClientId, Time.realtimeSinceStartup));
                idLastPingTime[nextClientId] = Time.time;
                clientIds.Add(id);

                idToIVKeyGenerator[id] = new Random(seed);

                SendToClient(new Ping(0).Serialize(), nextClientId);
                onNewClient?.Invoke(nextClientId);
                nextClientId++;
            }
        }

        protected void RemoveClient(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
            {
                Debug.Log("Removing client: " + ip.Address);
                clients.Remove(ipToId[ip]);
                idLastPingTime.Remove(ipToId[ip]);
                clientIds.Remove(ipToId[ip]);
                onClientRemoved?.Invoke(ipToId[ip]);
                ipToId.Remove(ip);
            }
        }

        protected virtual void RemoveClient(int id)
        {
            if (ipToId.ContainsValue(id))
            {
                Debug.Log("Removing client: " + id);
                ipToId.Remove(clients[id].ipEndPoint);
                clientIdToMessageId.Remove(id);
                idToIVKeyGenerator.Remove(id);
                clients.Remove(id);
                idLastPingTime.Remove(id);
                clientIds.Remove(id);
                onClientRemoved?.Invoke(id);
            }
        }

        public void SendToClient(byte[] data, int clientId)
        {
            Client client = clients[clientId];
            connection.Send(data, client.ipEndPoint);
        }

        public void Broadcast(byte[] data)
        {
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    connection.Send(data, iterator.Current.Value.ipEndPoint);
                }
            }
        }

        protected int GetReceivedClientId(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
                return ipToId[ip];

            return nextClientId;
        }

        private void PingCheck()
        {
            short[] clientsMs = new short[clientIds.Count];

            for (int i = 0; i < clientIds.Count; i++)
            {
                short ms = (short)((Time.time - idLastPingTime[clientIds[i]]) * 1000);
                clientsMs[i] = ms;
                if (ms > TimeOutTime * 1000)
                {
                    Broadcast(new Disconnect(clientIds[i]).Serialize());
                    RemoveClient(clientIds[i]);
                }
            }

            Broadcast(new AllPings(clientsMs, clientIds.Count).Serialize());
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