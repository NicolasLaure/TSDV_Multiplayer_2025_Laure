using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Network_dll.Messages.Data;
using Network.CheckSum;
using Network.Encryption;
using Network.Enums;
using Network.Messages;
using Network.Messages.MatchMaker;
using Network.Messages.Server;
using Network.Utilities;
using UnityEngine;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public class NetworkClient : NetworkManager<NetworkClient>
    {
        private readonly Dictionary<int, string> idToUsername = new Dictionary<int, string>();
        public Action<short> onPingUpdated;
        public Action<string> onError;
        public Action<ClientPing[]> onReceiveAllPings;

        private float ping = 0;
        private float lastPingTime;
        private int id;
        private float clientStartTime;
        private Random ivKeyGenerator;

        public Action<int> onClientDisconnect;
        public Action onDisconnection;
        public Action<string, byte[]> onChatMessageReceived;
        public int Id => id;

        public string username;
        private int _elo = 0;
        public short color;

        public float timeAfterGameOver = 5f;

        public void StartClient(IPAddress ip, int port, string username, short color)
        {
            Initialize();
            this.port = port;
            ipAddress = ip;
            lastPingTime = Time.time;
            ping = 0;
            this.username = username;
            this.color = color;

            connection = new UdpConnection(ip, port, this);
            clientStartTime = Time.time;

            HandshakeData handshakeData;
            handshakeData.usernameLength = this.username.Length;
            handshakeData.username = this.username;
            byte[] handshakeBytes = new PublicHandshake(handshakeData).Serialize();
            SendToServer(handshakeBytes);
            Debug.Log("Handshake Sent");
            heldMessages.Add(new HeldMessage(PublicHandshake.messageId, handshakeBytes));
        }

        public override void Update()
        {
            base.Update();
            ping = Time.time - lastPingTime;
            onPingUpdated?.Invoke((short)(ping * 1000));

            for (int i = 0; i < heldMessages.Count; i++)
            {
                if (ServerTime.time - heldMessages[i].heldSince >= maxResponseWait)
                {
                    Debug.Log($"Resending held message, Held Messages count: {heldMessages.Count}");
                    SendToServer(heldMessages[i].message);
                    heldMessages[i].heldSince = Time.time;
                }
            }

            if (connection != null && ping > TimeOutTime)
                EndClient();
        }

        public void EndClient()
        {
            if (connection == null)
                return;

            SendToServer(new Disconnect(id).Serialize());
            connection = null;
            onDisconnection?.Invoke();
        }

        public void SendToServer(byte[] data)
        {
            if (connection == null)
            {
                Debug.Log("CONNECTION NULL");
                return;
            }

            connection.Send(data);
        }

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            if (BitConverter.ToBoolean(data, 0))
            {
                data = Encrypter.Decrypt(ivKeyGenerator.Next(), data);
            }

            MessageType messageType = (MessageType)BitConverter.ToInt16(data, sizeof(bool));
            Attributes messageAttribs = (Attributes)BitConverter.ToInt16(data, sizeof(bool) + sizeof(short));

            if (messageAttribs.HasFlag(Attributes.Checksum))
            {
                if (!CheckSumCalculations.IsCheckSumOk(data))
                {
                    Debug.Log("CheckSum Not Okay");
                    return;
                }

                Debug.Log("CheckSum Okay");
            }

            if (messageType == MessageType.Ping)
            {
                Debug.Log("RECEIVED PING");
                lastPingTime = Time.time;
                SendToServer(new Ping(0).Serialize());
                return;
            }

            int receivedClientId = BitConverter.ToInt32(data, MessageOffsets.ClientIdIndex);
            int messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
            if (messageAttribs.HasFlag(Attributes.Order))
            {
                if (messageAttribs.HasFlag(Attributes.Important))
                {
                    if (messageId > ReadMessageId(receivedClientId, messageType) + 1)
                    {
                        Debug.Log("Intermediate Message Lost");
                        SaveHeldMessage(receivedClientId, messageType, messageId, data);
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
                case MessageType.Acknowledge:
                    Acknowledge acknowledgedMessage = new Acknowledge(data);
                    TryRemoveHeldMessage(acknowledgedMessage.acknowledgedType, acknowledgedMessage.acknowledgedId);
                    break;
                case MessageType.DisAcknowledge:
                    break;
                case MessageType.Disconnect:
                    Disconnect disconnect = new Disconnect(data);
                    if (disconnect.id == -1)
                    {
                        EndClient();
                        return;
                    }

                    onClientDisconnect?.Invoke(disconnect.id);
                    break;
                case MessageType.Chat:
                    if (idToUsername.ContainsKey(receivedClientId))
                        onChatMessageReceived?.Invoke(idToUsername[receivedClientId], data);
                    break;
                case MessageType.Error:
                    break;
                case MessageType.AllPings:
                    ClientsPing allClientsPing = new AllPings(data).clientsPing;
                    onReceiveAllPings?.Invoke(allClientsPing.clientPings);
                    break;
                // Moving Cubes message
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponse(new ServerHsResponse(data));
                    SendToServer(Encrypter.Encrypt(ivKeyGenerator.Next(), new PrivateHandshake(_elo, color).Serialize()));
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                case MessageType.PrivateHsResponse:
                    OnReceiveEvent?.Invoke(data, ip);
                    break;

                case MessageType.MatchMakerHsResponse:
                    HandleMatchMakerHandshakeResponse(new MatchMakerHsResponse(data));
                    SendToServer(Encrypter.Encrypt(ivKeyGenerator.Next(), new PrivateMatchMakerHandshake(_elo).Serialize()));
                    break;
                case MessageType.PrivateMatchmakerHsResponse:
                    PrivateMatchmakerHsResponse MmHsResponse = new PrivateMatchmakerHsResponse(data);
                    _elo = MmHsResponse.elo;
                    break;
                case MessageType.ServerDirection:
                    ServerDirection svDir = new ServerDirection(data);
                    Debug.Log($"ServerIp {svDir.serverIp}, port: {svDir.serverPort}");
                    StartClient(svDir.serverIp, svDir.serverPort, username, color);
                    break;
                case MessageType.Position:
                case MessageType.Crouch:
                case MessageType.InstantiateRequest:
                case MessageType.DeInstantiateRequest:
                case MessageType.Death:
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                case MessageType.Username:
                    HandleUsernameMessage(new UsernameMessage(data));
                    break;
                case MessageType.Usernames:
                    HandleUsernamesMessage(new UsernamesMessage(data));
                    break;


                case MessageType.Error_UsernameTaken:
                    Debug.LogError($"USERNAME WAS TAKEN");
                    onError?.Invoke("Username is already used");
                    EndClient();
                    break;
                case MessageType.Error_AfkDisconnect:
                    Debug.LogError($"AFK DISCONNECT");
                    onError?.Invoke("Afk");
                    EndClient();
                    break;
                case MessageType.Error_UserBanned:
                    Debug.LogError($"USER BANNED");
                    onError?.Invoke("UserWasBanned");
                    EndClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (messageAttribs.HasFlag(Attributes.Important))
                SendToServer(new Acknowledge(messageType, messageId).Serialize());

            if (messageAttribs.HasFlag(Attributes.Critical) && messageId > clientIdToMessageId[receivedClientId][messageType])
                SaveCriticalMessage(receivedClientId, messageId, data);

            SaveMessageId(receivedClientId, messageType, messageId);
            if (messageAttribs.HasFlag(Attributes.Order) && messageAttribs.HasFlag(Attributes.Important) && AreHeldMessages(receivedClientId, messageType))
            {
                HeldMessage oldestHeldMessage = heldImportantAndOrder[receivedClientId][messageType][0];
                if (oldestHeldMessage.id == messageId + 1)
                {
                    Debug.Log("Executing held message");
                    OnReceiveData(oldestHeldMessage.message, ip);
                }
            }
        }

        private IEnumerator PingTest(float delay)
        {
            yield return new WaitForSeconds(delay);
            SendToServer(new Ping(0).Serialize());
        }

        private void HandleMatchMakerHandshakeResponse(MatchMakerHsResponse data)
        {
            id = data.MatchMakerHandshakeData.id;
            seed = data.MatchMakerHandshakeData.seed;
            Debug.Log($"Seed: {Seed}");
            rngGenerator = new Random(seed);
            ivKeyGenerator = new Random(seed);
            OperationsList.Populate(rngGenerator);
        }

        private void HandleHandshakeResponse(ServerHsResponse data)
        {
            id = data.ServerHandshakeData.id;
            seed = data.ServerHandshakeData.seed;
            Debug.Log($"Seed: {Seed}");
            rngGenerator = new Random(seed);
            ivKeyGenerator = new Random(seed);
            OperationsList.Populate(rngGenerator);
        }

        private void HandleUsernamesMessage(UsernamesMessage usernamesMessage)
        {
            Debug.Log($"Usernames Count: {usernamesMessage.usernames.Length}");
            for (int i = 0; i < usernamesMessage.usernames.Length; i++)
            {
                HandleNewUsername(usernamesMessage.usernames[i]);
            }
        }

        private void HandleUsernameMessage(UsernameMessage usernamesMessage)
        {
            HandleNewUsername(usernamesMessage.username);
        }

        private void HandleNewUsername(OtherUsername usernamesMessage)
        {
            if (idToUsername.ContainsKey(usernamesMessage.id) && idToUsername[usernamesMessage.id] != "")
                return;

            idToUsername[usernamesMessage.id] = usernamesMessage.username;
        }

        public string GetUsername(int id)
        {
            if (idToUsername.ContainsKey(id))
                return idToUsername[id];

            Debug.Log($"Id {id} was not a key in dictionary");
            return "";
        }
    }
}