using System;
using System.Net;
using Network.Enums;
using Network.Messages;
using Network.Messages.TestMessages;
using UnityEngine;
using Ping = Network.Messages.Ping;

namespace Network
{
    public class NonAuthoritativeServer : NetworkServer<NonAuthoritativeServer>
    {
        public static Action onServerStart;

        private void OnEnable()
        {
            StartServer();
            onServerStart?.Invoke();
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
                    PrivateMatchMakerHandshake receivedMatchMakerHandshake = new PrivateMatchMakerHandshake(data);
                    Debug.Log($"Decrypted Private Handshake Elo {receivedMatchMakerHandshake.elo}");
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
    }
}