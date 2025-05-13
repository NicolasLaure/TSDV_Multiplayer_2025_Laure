using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Network.CheckSum;
using Network.Enums;
using Network.Messages;
using Network.Messages.MatchMaker;
using Network.Messages.TestMessages;
using UnityEngine;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public class MatchmakerManager : NetworkServer<MatchmakerManager>
    {
        private readonly Dictionary<int, int> clientIdToElo = new Dictionary<int, int>();
        private List<int> initializedClientIds = new List<int>();

        private void OnEnable()
        {
            StartServer();
        }

        protected override void Update()
        {
            base.Update();
            CheckQueue();
        }

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = GetReceivedClientId(ip);

            MessageType messageType;
            Attributes messageAttribs;
            if (BitConverter.ToBoolean(data, 0))
            {
                if (!CheckHeader(idToIVKeyGenerator[ipToId[ip]].Next(), out messageType, out messageAttribs, data, out byte[] decryptedData))
                    return;

                data = decryptedData;
            }
            else if (!CheckHeader(out messageType, out messageAttribs, data))
                return;

            if (messageType == MessageType.Ping)
            {
                short ms = (short)Mathf.FloorToInt((Time.time - idLastPingTime[receivedClientId]) * 1000);
                idLastPingTime[receivedClientId] = Time.time;
                SendToClient(new Ping(ms).Serialize(), receivedClientId);
                return;
            }

            int messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
            if (messageAttribs.HasFlag(Attributes.Order))
            {
                if (messageAttribs.HasFlag(Attributes.Important))
                {
                    if (messageId > ReadMessageId(receivedClientId, messageType) + 1)
                    {
                        Debug.Log("Intermediate Message Lost");
                        SaveHeldMessage(ipToId[ip], messageType, messageId, data);
                        return;
                    }
                }

                if (messageId <= ReadMessageId(receivedClientId, messageType))
                {
                    Debug.Log($"MessageId {messageId} was older than, message {ReadMessageId(receivedClientId, messageType)}");
                    return;
                }
            }

            switch (messageType)
            {
                case MessageType.HandShake:
                    if (ReadMessageId(receivedClientId, messageType) != messageId)
                    {
                        if (!ipToId.ContainsKey(ip))
                        {
                            AddClient(ip);
                            receivedClientId = ipToId[ip];
                            SendToClient(new MatchMakerHsResponse(receivedClientId, seed).Serialize(), receivedClientId);
                        }
                    }
                    else
                        Debug.Log("This Message Already taken");

                    break;
                case MessageType.PrivateMatchMakerHandshake:
                    PrivateMatchMakerHandshake receivedMatchMakerHandshake = new PrivateMatchMakerHandshake(data);
                    Debug.Log($"client{receivedClientId} Elo Is {receivedMatchMakerHandshake.elo}");
                    clientIdToElo[receivedClientId] = receivedMatchMakerHandshake.elo;
                    initializedClientIds.Add(receivedClientId);
                    break;
                case MessageType.Acknowledge:
                    break;
                case MessageType.Disconnect:
                    RemoveClient(ip);
                    Broadcast(data);
                    break;
                case MessageType.Error:
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

        private void CheckQueue()
        {
            if (initializedClientIds.Count > 0 && !clientIdToElo.ContainsKey(initializedClientIds[initializedClientIds.Count - 1]))
                return;

            if (initializedClientIds.Count >= 4)
            {
                for (int i = 0; i < initializedClientIds.Count; i++)
                {
                    Debug.Log($"Client[{initializedClientIds[i]}] has elo: {clientIdToElo[initializedClientIds[i]]}");
                }
            }
        }

        private void CreateServer()
        {
        }

        private void ConnectClients()
        {
        }
    }
}