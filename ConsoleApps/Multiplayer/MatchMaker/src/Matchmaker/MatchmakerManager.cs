using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Network_dll.Messages.ErrorMessages;
using Network.Encryption;
using Network.Enums;
using Network.Matchmaker;
using Network.Messages;
using Network.Messages.MatchMaker;
using Network.Utilities;

namespace Network
{
    public class MatchmakerManager : NetworkServer<MatchmakerManager>
    {
        private List<int> connectingToServerPairs = new List<int>();

        private int minWaitingClients = 2;
        private int maxEloDifference = 400;
        private int minSvPort = 60326;
        private int maxSvPort = 60350;
        private List<int> usedPorts = new List<int>();
        private List<ActiveServer> _activeServers = new List<ActiveServer>();

        public string nonAuthServerPath = "NonAuthoritativeServer.exe";
        public string authServerPath = "Server/Multiplayer.exe";

        private readonly Dictionary<HeldMessage, int> heldMessageToClientId = new Dictionary<HeldMessage, int>();

        public override void Update()
        {
            try
            {
                base.Update();
                CheckActiveServers();
                CheckQueue(true);
                CheckQueue(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(10000);
                throw;
            }
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

                if (receivedClientId == nextClientId && messageType != MessageType.HandShake)
                    return;

                if (messageType == MessageType.Ping)
                {
                    double ms = Math.Floor((ServerTime.time - clients[receivedClientId].lastPingTime) * 1000);
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
                        Logger.Log(messageId.ToString());
                        if (ReadMessageId(receivedClientId, messageType) != messageId)
                        {
                            if (!ipToId.ContainsKey(ip))
                            {
                                PublicHandshake handshake = new PublicHandshake(data);
                                if (!UsernameAvailable(handshake.handshakeData.username))
                                {
                                    SendToIp(new UsernameTaken().Serialize(), ip);
                                    return;
                                }

                                SavedClient clientData = _savedClientHandler.GetClientData(handshake.handshakeData.username);
                                if (clientData.isBanned)
                                {
                                    SendToIp(new UserIsBanned().Serialize(), ip);
                                    return;
                                }

                                AddClient(ip, handshake.handshakeData.username);
                                receivedClientId = ipToId[ip];
                                clients[receivedClientId].elo = clientData.elo;
                                clients[receivedClientId].isAuthServer = handshake.handshakeData.isAuthServer;

                                Logger.Log($"NEW CLIENT, Id: {receivedClientId}, Username: {clients[receivedClientId].username}, Elo:{clients[receivedClientId].elo}");
                                SendToClient(new MatchMakerHsResponse(receivedClientId, seed).Serialize(), receivedClientId);
                                heldMessages.Add(new HeldMessage(MatchMakerHsResponse.messageId, data));
                            }
                        }
                        else
                            Logger.Log("This Message Already taken");

                        break;
                    case MessageType.PrivateMatchMakerHandshake:
                        PrivateMatchMakerHandshake receivedMatchMakerHandshake = new PrivateMatchMakerHandshake(data);
                        clients[receivedClientId].isInitialized = true;
                        SendToClient(Encrypter.Encrypt(idToIVKeyGenerator[ipToId[ip]].Next(), new PrivateMatchmakerHsResponse(clients[receivedClientId].elo).Serialize()), ipToId[ip]);
                        break;
                    case MessageType.Acknowledge:
                        Acknowledge acknowledgedMessage = new Acknowledge(data);
                        if (TryGetHeldMessage(acknowledgedMessage.acknowledgedType, acknowledgedMessage.acknowledgedId, out HeldMessage heldMessage) && heldMessageToClientId.ContainsKey(heldMessage))
                            RemoveClient(heldMessageToClientId[heldMessage]);

                        TryRemoveHeldMessage(acknowledgedMessage.acknowledgedType, acknowledgedMessage.acknowledgedId);
                        break;
                    case MessageType.Disconnect:
                        if (ipToId.ContainsKey(ip))
                            RemoveClient(ipToId[ip]);
                        Broadcast(data);
                        break;
                    case MessageType.Error:
                        break;
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

        protected override void RemoveClient(int id)
        {
            base.RemoveClient(id);

            if (connectingToServerPairs.Contains(id))
                connectingToServerPairs.Remove(id);
        }

        private void CheckQueue(bool isAuthServer)
        {
            List<Client> waitingClients = GetWaitingClients(isAuthServer);

            if (waitingClients.Count >= minWaitingClients)
            {
                for (int i = 0; i < minWaitingClients; i += 2)
                {
                    PairPlayers(isAuthServer);
                }
            }
        }

        private List<Client> GetWaitingClients(bool isAuthSv)
        {
            List<Client> waitingClients = new List<Client>();
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    Client client = iterator.Current.Value;
                    if (client.isInitialized && !client.isConnectedToServer && client.isAuthServer == isAuthSv)
                        waitingClients.Add(client);
                }
            }

            return waitingClients;
        }

        private void PairPlayers(bool isAuthServer)
        {
            if (TryFindPairs(isAuthServer, out int idOne, out int idTwo))
            {
                Logger.Log($"Pair A: {idOne}, B: {idTwo}");
                connectingToServerPairs.Add(idOne);
                connectingToServerPairs.Add(idTwo);
                CreateServer(isAuthServer, idOne, idTwo);
            }
        }

        private bool TryFindPairs(bool isAuthServer, out int player1Id, out int player2Id)
        {
            List<Client> initializedClientIds = GetWaitingClients(isAuthServer);
            for (int i = 0; i < initializedClientIds.Count; i++)
            {
                for (int j = 0; j < initializedClientIds.Count; j++)
                {
                    if (i == j)
                        continue;

                    if (Math.Abs(initializedClientIds[i].elo - initializedClientIds[j].elo) <= maxEloDifference)
                    {
                        player1Id = initializedClientIds[i].id;
                        player2Id = initializedClientIds[j].id;
                        return true;
                    }
                }
            }

            Logger.Log("Couldn't Find matching pair");
            player1Id = -1;
            player2Id = -1;
            return false;
        }

        private void CreateServer(bool isAuthServer, int clientOneId, int clientTwoId)
        {
            int newSvPort = GetMinUnusedPort();
            usedPorts.Add(newSvPort);

            string path = Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(path);
            string serverPath = Path.Combine(directory, nonAuthServerPath);
            if (isAuthServer)
                serverPath = Path.Combine(directory, authServerPath);

            ProcessStartInfo serverInfo = new ProcessStartInfo(serverPath, newSvPort.ToString());
            serverInfo.UseShellExecute = true;
            Process newServerProcess = Process.Start(serverInfo);

            int sleepMs = 2000;
            if (isAuthServer)
                sleepMs = 10000;

            Thread.Sleep(sleepMs);
            byte[] newSvDirection1 = new ServerDirection(ipAddress, newSvPort).Serialize();
            SaveHeldMessage(newSvDirection1, clientOneId);
            byte[] newSvDirection2 = new ServerDirection(ipAddress, newSvPort).Serialize();
            SaveHeldMessage(newSvDirection2, clientTwoId);

            SendToClient(newSvDirection1, clientOneId);
            SendToClient(newSvDirection2, clientTwoId);

            List<Client> connectedPair = new List<Client>();
            connectedPair.Add(clients[clientOneId]);
            connectedPair.Add(clients[clientTwoId]);
            clients[clientTwoId].isConnectedToServer = true;
            clients[clientOneId].isConnectedToServer = true;
            _activeServers.Add(new ActiveServer(connectedPair, ipAddress, newSvPort, newServerProcess));
        }

        private int GetMinUnusedPort()
        {
            for (int i = minSvPort; i < maxSvPort; i++)
            {
                bool freePort = true;

                for (int j = 0; j < usedPorts.Count; j++)
                {
                    if (i == usedPorts[j])
                        freePort = false;
                }

                if (freePort)
                    return i;
            }

            throw new Exception("Not Enough Free Ports");
        }

        private void SaveHeldMessage(byte[] data, int clientId)
        {
            Logger.Log($"MessageId {ServerDirection.messageId}");
            HeldMessage held = new HeldMessage(ServerDirection.messageId, data);
            heldMessages.Add(held);
            heldMessageToClientId[held] = clientId;
        }

        private void CheckActiveServers()
        {
            List<ActiveServer> serversToRemove = new List<ActiveServer>();
            for (int i = 0; i < _activeServers.Count; i++)
            {
                if (_activeServers[i].process.HasExited)
                {
                    usedPorts.Remove(_activeServers[i].serverPort);
                    serversToRemove.Add(_activeServers[i]);
                    Logger.Log($"Process[{i}] was closed!");
                }
            }

            for (int i = 0; i < serversToRemove.Count; i++)
            {
                _activeServers.Remove(serversToRemove[i]);
            }
        }

        private bool UsernameAvailable(string username)
        {
            using (var iterator = clients.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.username == username)
                        return false;
                }
            }

            foreach (ActiveServer server in _activeServers)
            {
                foreach (Client client in server.clients)
                {
                    if (client.username == username)
                        return false;
                }
            }

            return true;
        }
    }
}