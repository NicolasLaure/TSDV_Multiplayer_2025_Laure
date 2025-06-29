using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Network_dll.Messages.ClientMessages;
using Network_dll.Messages.ErrorMessages;
using Network.ChatSafety;
using Network.Elo;
using Network.Encryption;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using Network.Messages.TestMessages;
using Network.Utilities;
using UnityEngine;
using Logger = Network.Utilities.Logger;
using Ping = Network.Messages.Ping;

namespace Network
{
    public class AuthoritativeServer : NetworkServer<AuthoritativeServer>
    {
        public static Action onServerStart;
        private ChatGuardian _chatGuardian;
        private EloHandler _eloHandler;
        private AuthServerFactory _serverFactory;
        private readonly Dictionary<int, List<InstanceData>> instanceIdTointegrityChecks = new Dictionary<int, List<InstanceData>>();

        private float maxEmptyTime = 3;
        private float emptySince = 0;

        public AuthoritativeServer(AuthServerFactory serverFactory)
        {
            _serverFactory = serverFactory;
        }

        public void Start(int port)
        {
            //_chatGuardian = new ChatGuardian();
            _eloHandler = new EloHandler(_savedClientHandler);
            StartServer(port);
            onServerStart?.Invoke();

            emptySince = ServerTime.time;
        }

        public override void Update()
        {
            if (clients.Count == 0 && ServerTime.time - emptySince >= maxEmptyTime)
            {
                EndServer();
            }

            base.Update();
            AfkCheck();
        }

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            try
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
                    short ms = (short)Math.Floor((ServerTime.time - clients[receivedClientId].lastPingTime * 1000));
                    clients[receivedClientId].lastPingTime = ServerTime.time;
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
                            Logger.Log("Intermediate Message Lost");
                            SaveHeldMessage(ipToId[ip], messageType, messageId, data);
                            return;
                        }
                    }

                    if (messageId <= ReadMessageId(receivedClientId, messageType))
                    {
                        Logger.Log($"MessageId {messageId} was older than, message {ReadMessageId(receivedClientId, messageType)}");
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
                                PublicHandshake handshake = new PublicHandshake(data);
                                Debug.Log("HandhsakeReceived");
                                AddClient(ip, handshake.handshakeData.username);
                                receivedClientId = ipToId[ip];
                            }
                        }
                        else
                            Logger.Log("This Message Already taken");

                        break;
                    case MessageType.PrivateHandshake:
                        PrivateHandshake receivedMatchMakerHandshake = new PrivateHandshake(data);
                        Logger.Log($"Decrypted Private Handshake Color {receivedMatchMakerHandshake.color}");
                        InstantiateAll objectsToInstantiate = _serverFactory.GetObjectsToInstantiate();
                        PrivateServerHsResponse response = new PrivateServerHsResponse(objectsToInstantiate);
                        SendToClient(Encrypter.Encrypt(idToIVKeyGenerator[receivedClientId].Next(), response.Serialize()), receivedClientId);
                        SendToClient(new UsernamesMessage(GetAllUsernames()).Serialize(), receivedClientId);
                        Broadcast(new UsernameMessage(GetUserName(receivedClientId)).Serialize());
                        OnReceiveEvent?.Invoke(data, ip);
                        break;
                    case MessageType.Acknowledge:
                        break;
                    case MessageType.DisAcknowledge:
                        break;
                    case MessageType.Disconnect:
                        RemoveClient(ip);
                        Broadcast(data);
                        if (clients.Count == 0)
                            emptySince = ServerTime.time;
                        break;
                    case MessageType.Error:
                        break;
                    case MessageType.Chat:
                        Chat chatMessage = new Chat(data);
                        if (_chatGuardian.IsSafeMessage(chatMessage.message))
                        {
                            Broadcast(data);
                            SetPlayerActive(receivedClientId);
                        }
                        else
                        {
                            SavedClient clientData = _savedClientHandler.GetClientData(clients[receivedClientId].username);
                            clientData.isBanned = true;
                            _savedClientHandler.SaveClient(clientData);
                            SendToClient(new UserIsBanned().Serialize(), receivedClientId);
                        }

                        break;
                    case MessageType.Position:
                    case MessageType.Crouch:

                        SetPlayerActive(receivedClientId);
                        break;
                    case MessageType.ImportantOrderTest:
                        Logger.Log($"ImportantOrder: {new ImportantOrderMessage(data).number}");
                        break;
                    case MessageType.InstantiateRequest:
                        break;
                    case MessageType.DeInstantiateRequest:
                        break;

                    case MessageType.AxisInput:
                    case MessageType.ActionInput:
                        OnReceiveEvent?.Invoke(data, ip);
                        break;

                    case MessageType.Death:
                        Broadcast(data);
                        break;
                    case MessageType.Win:
                        Win win = new Win(data);
                        Logger.Log($"Player {win.winnerUsername} won the game");
                        _eloHandler.EloCalculation(win.winnerUsername, win.loserUsername);
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
                        Logger.Log("Executing held message");
                        OnReceiveData(oldestHeldMessage.message, ip);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(10000);
                throw;
            }
        }

        protected void SetPlayerActive(int id)
        {
            clients[id].lastActiveTime = ServerTime.time;
        }

        protected void AfkCheck()
        {
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    Client client = iterator.Current.Value;
                    float afkTime = (ServerTime.time - client.lastActiveTime);

                    if (afkTime > afkDisconnectTime)
                    {
                        SendToClient(new AfkDisconnect().Serialize(), client.id);
                        Thread.Sleep(250);
                        Broadcast(new Disconnect(client.id).Serialize());
                    }
                }
            }
        }

        protected OtherUsername GetUserName(int id)
        {
            OtherUsername username;
            username.id = clients[id].id;
            username.usernameLength = clients[id].username.Length;
            username.username = clients[id].username;
            Logger.Log($"Got User:{username.id}, name={username.username}");
            return username;
        }

        protected OtherUsername[] GetAllUsernames()
        {
            List<OtherUsername> usernames = new List<OtherUsername>();
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    Client client = iterator.Current.Value;
                    OtherUsername username;
                    username.id = client.id;
                    username.username = client.username;
                    Logger.Log($"USERNAME CLIENT[{client.id}] = {username.username}");
                    username.usernameLength = client.username.Length;
                    usernames.Add(username);
                }
            }

            return usernames.ToArray();
        }
    }
}