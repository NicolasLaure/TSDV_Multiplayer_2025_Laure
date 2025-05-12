using System;
using System.Collections.Generic;
using System.Net;
using Network.CheckSum;
using Network.Encryption;
using Network.Enums;
using Network.Messages;
using Network.Messages.TestMessages;
using Network.SaveStructures;
using UnityEngine;
using UnityEngine.Timeline;
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
        private readonly List<int> clientIds = new List<int>();
        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
        private readonly Dictionary<int, float> idLastPingTime = new Dictionary<int, float>();

        private readonly Dictionary<int, Dictionary<MessageType, int>> clientIdToMessageId = new Dictionary<int, Dictionary<MessageType, int>>();
        private readonly Dictionary<int, Dictionary<MessageType, List<HeldMessage>>> heldImportantAndOrder = new Dictionary<int, Dictionary<MessageType, List<HeldMessage>>>();
        private readonly PublicHandshakeResponse _heldPublicHandshakeSa;

        public Action<int> onNewClient;
        public Action<int> onClientRemoved;

        private int nextClientId = 0; // This id should be generated during first handshake

        private readonly Dictionary<int, Random> idToIVKeyGenerator = new Dictionary<int, Random>();

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

        protected override void Update()
        {
            base.Update();
            PingCheck();
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
                idLastPingTime[nextClientId] = Time.time;
                clientIds.Add(id);

                idToIVKeyGenerator[id] = new Random(seed);

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
                idLastPingTime.Remove(ipToId[ip]);
                clientIds.Remove(ipToId[ip]);
                onClientRemoved?.Invoke(ipToId[ip]);
                ipToId.Remove(ip);
            }
        }

        void RemoveClient(int id)
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

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = -1;
            if (ipToId.ContainsKey(ip))
                receivedClientId = ipToId[ip];
            else
                receivedClientId = nextClientId;

            if (BitConverter.ToBoolean(data, 0))
                data = Encrypter.Decrypt(idToIVKeyGenerator[ipToId[ip]].Next(), data);

            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            Attributes messageAttribs = (Attributes)BitConverter.ToInt16(data, MessageOffsets.AttribsIndex);

            if (messageAttribs.HasFlag(Attributes.Checksum))
            {
                if (!CheckSumCalculations.IsCheckSumOk(data))
                {
                    Debug.Log("CheckSum Not Okay");
                    return;
                }

                Debug.Log("CheckSum Okay");
            }

            int messageId = -1;
            if (messageType != MessageType.Ping)
            {
                messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
                TryInitializeMessageId(receivedClientId, messageType);

                if (messageAttribs.HasFlag(Attributes.Order))
                {
                    if (messageAttribs.HasFlag(Attributes.Important))
                    {
                        if (messageId > clientIdToMessageId[receivedClientId][messageType] + 1)
                        {
                            Debug.Log("Intermediate Message Lost");
                            SaveHeldMessage(ipToId[ip], messageType, messageId, data);
                            return;
                        }
                    }

                    if (messageId <= clientIdToMessageId[receivedClientId][messageType])
                    {
                        Debug.Log($"MessageId {messageId} was older than, message {clientIdToMessageId[receivedClientId][messageType]}");
                        return;
                    }
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
                case MessageType.PrivateHandshake:
                    PrivateHandshake receivedHandshake = new PrivateHandshake(data);
                    Debug.Log($"Decrypted Private Handshake id {receivedHandshake.id}");
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
                    short ms = (short)Mathf.FloorToInt((Time.time - idLastPingTime[receivedClientId]) * 1000);
                    idLastPingTime[receivedClientId] = Time.time;
                    SendToClient(new Ping(ms).Serialize(), receivedClientId);
                    return;
                    break;

                case MessageType.Position:
                    Broadcast(data);
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                case MessageType.ImportantOrderTest:
                    Debug.Log($"ImportantOrder: {new ImportantOrderMessage(data).number}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (messageAttribs.HasFlag(Attributes.Important))
                SendToClient(new Acknowledge(messageType, messageId).Serialize(), receivedClientId);

            if (messageAttribs.HasFlag(Attributes.Critical) && messageId > clientIdToMessageId[receivedClientId][messageType])
                SaveCriticalMessage(ipToId[ip], messageId, data);

            SaveMessageId(receivedClientId, messageType, messageId);
            if (messageAttribs.HasFlag(Attributes.Order) && messageAttribs.HasFlag(Attributes.Important) && AreHeldMessages(ipToId[ip], messageType))
            {
                HeldMessage oldestHeldMessage = heldImportantAndOrder[ipToId[ip]][messageType][0];
                if (oldestHeldMessage.id == messageId + 1)
                {
                    Debug.Log("Executing held message");
                    OnReceiveData(oldestHeldMessage.message, ip);
                }
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

        public int GetIpId(IPEndPoint ip)
        {
            if (ipToId.ContainsKey(ip))
                return ipToId[ip];

            return -1;
        }

        private void TryInitializeMessageId(int clientId, MessageType type)
        {
            if (!clientIdToMessageId.ContainsKey(clientId))
                clientIdToMessageId.Add(clientId, new Dictionary<MessageType, int>());

            if (!clientIdToMessageId[clientId].ContainsKey(type))
                clientIdToMessageId[clientId][type] = -1;
        }

        private void SaveMessageId(int clientId, MessageType type, int messageId)
        {
            if (!clientIdToMessageId.ContainsKey(clientId))
                clientIdToMessageId.Add(clientId, new Dictionary<MessageType, int>());
            if (!clientIdToMessageId[clientId].ContainsKey(type))
                clientIdToMessageId[clientId][type] = -1;

            clientIdToMessageId[clientId][type] = messageId > clientIdToMessageId[clientId][type] ? messageId : clientIdToMessageId[clientId][type];
        }

        private void SaveHeldMessage(int clientId, MessageType type, int messageId, byte[] message)
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


        private bool AreHeldMessages(int clientId, MessageType messageType)
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