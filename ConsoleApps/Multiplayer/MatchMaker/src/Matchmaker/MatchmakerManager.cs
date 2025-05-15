using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Network.Enums;
using Network.Matchmaker;
using Network.Messages;
using Network.Messages.MatchMaker;
using Network.Utilities;

namespace Network
{
    public class MatchmakerManager : NetworkServer<MatchmakerManager>
    {
        private readonly Dictionary<int, int> clientIdToElo = new Dictionary<int, int>();
        private List<int> initializedClientIds = new List<int>();
        private List<int> connectingToServerPairs = new List<int>();

        private int minSvPort = 60326;
        private int maxSvPort = 60350;
        private List<int> usedPorts = new List<int>();
        private List<ActiveServer> _activeServers = new List<ActiveServer>();

        public string serverPath = "NonAuthoritativeServer.exe";

        private readonly Dictionary<HeldMessage, int> heldMessageToClientId = new Dictionary<HeldMessage, int>();

        private List<Process> runningServers = new List<Process>();
        private readonly Dictionary<Process, int> processToPort = new Dictionary<Process, int>();

        public override void Update()
        {
            base.Update();
            CheckActiveServers();
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

            if (receivedClientId == nextClientId && messageType != MessageType.HandShake)
                return;

            if (messageType == MessageType.Ping)
            {
                double ms = Math.Floor((ServerTime.time - idLastPingTime[receivedClientId]) * 1000);
                idLastPingTime[receivedClientId] = ServerTime.time;
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
                            Logger.Log("NEW CLIENT");
                            AddClient(ip);
                            receivedClientId = ipToId[ip];
                            SendToClient(new MatchMakerHsResponse(receivedClientId, seed).Serialize(), receivedClientId);
                            heldMessages.Add(new HeldMessage(MatchMakerHsResponse.messageId, data));
                        }
                    }
                    else
                        Logger.Log("This Message Already taken");

                    break;
                case MessageType.PrivateMatchMakerHandshake:
                    PrivateMatchMakerHandshake receivedMatchMakerHandshake = new PrivateMatchMakerHandshake(data);
                    Logger.Log($"client{receivedClientId} Elo Is {receivedMatchMakerHandshake.elo}");
                    clientIdToElo[receivedClientId] = receivedMatchMakerHandshake.elo;
                    initializedClientIds.Add(receivedClientId);
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

        protected override void RemoveClient(int id)
        {
            base.RemoveClient(id);

            if (initializedClientIds.Contains(id))
                initializedClientIds.Remove(id);
            if (connectingToServerPairs.Contains(id))
                connectingToServerPairs.Remove(id);

            if (clientIdToElo.ContainsKey(id))
                clientIdToElo.Remove(id);
        }

        private void CheckQueue()
        {
            if (initializedClientIds.Count > 0 && !clientIdToElo.ContainsKey(initializedClientIds[initializedClientIds.Count - 1]))
                return;

            if (initializedClientIds.Count >= 2)
            {
                connectingToServerPairs = initializedClientIds.GetRange(0, 2);
                initializedClientIds.RemoveRange(0, 2);
                CreateServer();
            }
        }

        private void CreateServer()
        {
            int newSvPort = GetMinUnusedPort();
            usedPorts.Add(newSvPort);

            ProcessStartInfo serverInfo = new ProcessStartInfo(serverPath, newSvPort.ToString());
            serverInfo.UseShellExecute = true;
            Process newServer = Process.Start(serverInfo);
            runningServers.Add(newServer);

            processToPort[newServer] = newSvPort;

            byte[] newSvDirection1 = new ServerDirection(ipAddress, newSvPort).Serialize();
            SaveHeldMessage(newSvDirection1, connectingToServerPairs[0]);
            byte[] newSvDirection2 = new ServerDirection(ipAddress, newSvPort).Serialize();
            SaveHeldMessage(newSvDirection2, connectingToServerPairs[1]);

            SendToClient(newSvDirection1, connectingToServerPairs[0]);
            SendToClient(newSvDirection2, connectingToServerPairs[1]);

            _activeServers.Add(new ActiveServer(connectingToServerPairs.GetRange(0, 2), ipAddress, newSvPort));
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
            List<Process> processesToRemove = new List<Process>();
            for (int i = 0; i < runningServers.Count; i++)
            {
                if (runningServers[i].HasExited)
                {
                    usedPorts.Remove(processToPort[runningServers[i]]);
                    processToPort.Remove(runningServers[i]);
                    processesToRemove.Add(runningServers[i]);
                    Logger.Log($"Process[{i}] was already closed");
                }
            }

            for (int i = 0; i < processesToRemove.Count; i++)
            {
                runningServers.Remove(processesToRemove[i]);
            }
        }
    }
}