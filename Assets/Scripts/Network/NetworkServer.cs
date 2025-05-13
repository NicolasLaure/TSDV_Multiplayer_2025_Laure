using System;
using System.Collections.Generic;
using System.Net;
using Network.CheckSum;
using Network.Enums;
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

        protected readonly Dictionary<int, Dictionary<MessageType, int>> clientIdToMessageId = new Dictionary<int, Dictionary<MessageType, int>>();
        protected readonly Dictionary<int, Dictionary<MessageType, List<HeldMessage>>> heldImportantAndOrder = new Dictionary<int, Dictionary<MessageType, List<HeldMessage>>>();
        protected readonly ServerHsResponse HeldServerHsSa;

        public Action<int> onNewClient;
        public Action<int> onClientRemoved;

        protected int nextClientId = 0; // This id should be generated during first handshake

        protected readonly Dictionary<int, Random> idToIVKeyGenerator = new Dictionary<int, Random>();

        public void StartServer()
        {
            port = defaultPort;
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

        protected void RemoveClient(int id)
        {
            if (ipToId.ContainsValue(id))
            {
                Debug.Log("Removing client: " + id);
                ipToId.Remove(clients[id].ipEndPoint);
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

        protected void SaveMessageId(int clientId, MessageType type, int messageId)
        {
            if (!clientIdToMessageId.ContainsKey(clientId))
                clientIdToMessageId.Add(clientId, new Dictionary<MessageType, int>());
            if (!clientIdToMessageId[clientId].ContainsKey(type))
                clientIdToMessageId[clientId][type] = -1;

            clientIdToMessageId[clientId][type] = messageId > clientIdToMessageId[clientId][type] ? messageId : clientIdToMessageId[clientId][type];
        }

        protected int ReadMessageId(int clientId, MessageType type)
        {
            if (!clientIdToMessageId.ContainsKey(clientId))
                clientIdToMessageId.Add(clientId, new Dictionary<MessageType, int>());
            if (!clientIdToMessageId[clientId].ContainsKey(type))
                clientIdToMessageId[clientId][type] = -1;

            return clientIdToMessageId[clientId][type];
        }

        protected void SaveHeldMessage(int clientId, MessageType type, int messageId, byte[] message)
        {
            if (!heldImportantAndOrder.ContainsKey(clientId))
                heldImportantAndOrder[clientId] = new Dictionary<MessageType, List<HeldMessage>>();
            if (!heldImportantAndOrder[clientId].ContainsKey(type))
                heldImportantAndOrder[clientId][type] = new List<HeldMessage>();

            int messageCount = heldImportantAndOrder[clientId][type].Count;
            for (int i = 0; i < messageCount; i++)
            {
                if (messageId < heldImportantAndOrder[clientId][type][i].id)
                {
                    heldImportantAndOrder[clientId][type].Insert(i, new HeldMessage(messageId, message));
                    return;
                }
            }

            heldImportantAndOrder[clientId][type].Add(new HeldMessage(messageId, message));
        }


        protected bool AreHeldMessages(int clientId, MessageType messageType)
        {
            return heldImportantAndOrder.ContainsKey(clientId) && heldImportantAndOrder[clientId].ContainsKey(messageType) && heldImportantAndOrder[clientId][messageType].Count > 0;
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
    }
}