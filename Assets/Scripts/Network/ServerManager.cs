using System;
using System.Collections.Generic;
using System.Net;
using Network.CheckSum;
using Network.Enums;
using Network.Messages;
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

    public class ServerManager : NetworkManager<ServerManager>
    {
        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
        private readonly Dictionary<int, float> idPingTime = new Dictionary<int, float>();

        private readonly Dictionary<int, Dictionary<MessageType, int>> clientIdToMessageId = new Dictionary<int, Dictionary<MessageType, int>>();
        private readonly HandshakeResponse heldHandshakeSa;

        public Action<int> onNewClient;
        public Action<int> onClientRemoved;

        private int nextClientId = 0; // This id should be generated during first handshake

        public void StartServer(int port)
        {
            this.port = port;
            connection = new UdpConnection(port, this);
            rngGenerator = new Random(Time.frameCount);
            seed = rngGenerator.Next(0, int.MaxValue);
            rngGenerator = new Random(seed);
            Debug.Log($"Server Seed: {seed}");

            OperationsList.Populate(rngGenerator);
        }

        void AddClient(IPEndPoint ip)
        {
            //ipToId does not contain a previous connected Ip? Has something to do with creating a new Connection? 
            if (!ipToId.ContainsKey(ip))
            {
                int id = nextClientId;
                ipToId[ip] = nextClientId;
                Debug.Log("Adding client: " + ip.Address + " ID: " + id);
                clients.Add(nextClientId, new Client(ip, nextClientId, Time.realtimeSinceStartup));
                idPingTime[nextClientId] = Time.time;
                SendToClient(new Ping(0).Serialize(), nextClientId);

                onNewClient?.Invoke(nextClientId);
                nextClientId++;
            }
        }

        void RemoveClient(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
            {
                Debug.Log("Removing client: " + ip.Address);
                clients.Remove(ipToId[ip]);
                onClientRemoved?.Invoke(ipToId[ip]);
            }
        }

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = -1;
            if (ipToId.ContainsKey(ip))
                receivedClientId = ipToId[ip];
            else
                receivedClientId = nextClientId;

            MessageType messageType = (MessageType)BitConverter.ToInt16(data, 0);
            Attributes messageAttribs = (Attributes)BitConverter.ToInt16(data, 2);
            int messageId = -1;
            if (messageType != MessageType.Ping)
            {
                messageId = BitConverter.ToInt32(data, sizeof(short) * 2);
                InitializeMessageId(receivedClientId, messageType);
                if (messageAttribs == Attributes.Order && messageId <= clientIdToMessageId[receivedClientId][messageType])
                {
                    Debug.Log($"MessageId {messageId} was older than, message {clientIdToMessageId[receivedClientId][messageType]}");
                    return;
                }
            }


            switch (messageType)
            {
                case MessageType.HandShake:
                    if (clientIdToMessageId[receivedClientId][messageType] != messageId)
                    {
                        if (!ipToId.ContainsKey(ip))
                        {
                            AddClient(ip);
                            receivedClientId = ipToId[ip];
                        }
                    }
                    else
                        Debug.Log("This Message Already taken");

                    break;
                case MessageType.Acknowledge:
                    break;
                case MessageType.DisAcknowledge:
                    break;
                case MessageType.Disconnect:
                    RemoveClient(ip);
                    Broadcast(data);
                    break;
                case MessageType.Error:
                    break;
                case MessageType.Ping:
                    short ms = (short)Mathf.FloorToInt((Time.time - idPingTime[receivedClientId]) * 1000);
                    idPingTime[receivedClientId] = Time.time;
                    SendToClient(new Ping(ms).Serialize(), receivedClientId);
                    return;
                    break;

                case MessageType.Position:
                    Broadcast(data);
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (messageAttribs == Attributes.Important)
                SendToClient(new Acknowledge(messageType, messageId).Serialize(), receivedClientId);

            SaveMessageId(receivedClientId, messageType, messageId);
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

        public int GetIpId(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
                return ipToId[ip];

            return -1;
        }

        private void InitializeMessageId(int clientId, MessageType type)
        {
            if (!clientIdToMessageId.ContainsKey(clientId))
            {
                Debug.Log($"ClientID: {clientId}");
                clientIdToMessageId.Add(clientId, new Dictionary<MessageType, int>());
                clientIdToMessageId[clientId][type] = -1;
            }
            else if (!clientIdToMessageId[clientId].ContainsKey(type))
            {
                clientIdToMessageId[clientId][type] = -1;
            }
        }

        private void SaveMessageId(int clientId, MessageType type, int messageId)
        {
            if (clientIdToMessageId.ContainsKey(clientId) && clientIdToMessageId[clientId].ContainsKey(type))
                clientIdToMessageId[clientId][type] = messageId > clientIdToMessageId[clientId][type] ? messageId : clientIdToMessageId[clientId][type];
        }
    }
}