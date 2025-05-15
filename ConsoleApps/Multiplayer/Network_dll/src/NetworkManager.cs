using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Network.CheckSum;
using Network.Encryption;
using Network.Enums;
using Network.Messages;
using Network.SaveStructures;
using Network.Utilities;
using Random = System.Random;

namespace Network
{
    public abstract class NetworkManager<T> : IReceiveData where T : NetworkManager<T>
    {
        public IPAddress ipAddress { get; protected set; }
        public int port { get; protected set; }

        public Action<byte[], IPEndPoint> OnReceiveEvent;
        protected UdpConnection connection;
        protected float maxResponseWait = 0.5f;
        protected int TimeOutTime = 10;

        protected int seed;
        protected Random rngGenerator;
        protected readonly List<CriticalMessage> criticalMessages = new List<CriticalMessage>();

        protected readonly Dictionary<int, Dictionary<MessageType, int>> clientIdToMessageId = new Dictionary<int, Dictionary<MessageType, int>>();
        protected readonly Dictionary<int, Dictionary<MessageType, List<HeldMessage>>> heldImportantAndOrder = new Dictionary<int, Dictionary<MessageType, List<HeldMessage>>>();

        protected readonly Dictionary<int, Random> idToIVKeyGenerator = new Dictionary<int, Random>();

        protected List<HeldMessage> heldMessages = new List<HeldMessage>();

        public int defaultPort = 60325;

        public int Seed => seed;

        public abstract void OnReceiveData(byte[] data, IPEndPoint ip);

        public virtual void Update()
        {
            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();
        }

        protected void SaveCriticalMessage(int clientId, int messageId, byte[] message)
        {
            criticalMessages.Add(new CriticalMessage(clientId, messageId, message));
        }

        protected bool CheckHeader(out MessageType type, out Attributes attributes, byte[] data)
        {
            type = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            attributes = (Attributes)BitConverter.ToInt16(data, MessageOffsets.AttribsIndex);

            if (attributes.HasFlag(Attributes.Checksum))
            {
                if (!CheckSumCalculations.IsCheckSumOk(data))
                {
                    Logger.Log("CheckSum Not Okay");
                    return false;
                }

                Logger.Log("CheckSum Okay");
            }

            return true;
        }

        protected bool CheckHeader(int IvKey, out MessageType type, out Attributes attributes, byte[] rawData, out byte[] data)
        {
            data = Encrypter.Decrypt(IvKey, rawData);

            return CheckHeader(out type, out attributes, data);
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

        protected bool TryGetHeldMessage(MessageType type, int messageId, out HeldMessage heldMessage)
        {
            for (int i = 0; i < heldMessages.Count; i++)
            {
                if (heldMessages[i].id == messageId && (MessageType)BitConverter.ToInt16(heldMessages[i].message, MessageOffsets.MessageTypeIndex) == type)
                {
                    heldMessage = heldMessages[i];
                    return true;
                }
            }

            heldMessage = null;
            return false;
        }

        protected bool TryRemoveHeldMessage(MessageType type, int messageId)
        {
            for (int i = 0; i < heldMessages.Count; i++)
            {
                if (heldMessages[i].id == messageId && (MessageType)BitConverter.ToInt16(heldMessages[i].message, MessageOffsets.MessageTypeIndex) == type)
                {
                    heldMessages.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}