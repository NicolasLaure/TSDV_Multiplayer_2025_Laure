using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Network.CheckSum;
using Network.Encryption;
using Network.Enums;
using Network.Messages;
using Network.Messages.TestMessages;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public class ClientManager : NetworkManager<ClientManager>
    {
        public Action<short> onPingUpdated;

        private float ping = 0;
        private float lastPingTime;
        private int id;
        private float clientStartTime;
        private Random ivKeyGenerator;

        public Action<int> onClientDisconnect;
        public Action onDisconnection;
        public int Id => id;

        private List<HeldMessage> _heldMessages = new List<HeldMessage>();

        public void StartClient(IPAddress ip, int port)
        {
            this.port = port;
            this.ipAddress = ip;
            lastPingTime = Time.time;
            ping = 0;

            connection = new UdpConnection(ip, port, this);
            clientStartTime = Time.time;

            HandshakeData handshakeData;
            handshakeData.ip = 0;
            byte[] handshakeBytes = new PublicHandshake(handshakeData, 0).Serialize();
            SendToServer(handshakeBytes);
            _heldMessages.Add(new HeldMessage(0, handshakeBytes));
        }

        protected override void Update()
        {
            base.Update();
            ping = Time.time - lastPingTime;
            onPingUpdated?.Invoke((short)(ping * 1000));

            for (int i = 0; i < _heldMessages.Count; i++)
            {
                if (Time.time - _heldMessages[i].heldSince >= maxResponseWait)
                {
                    Debug.Log($"Resending held message, Held Messages count: {_heldMessages.Count}");
                    SendToServer(_heldMessages[i].message);
                    _heldMessages[i].heldSince = Time.time;
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
            Instance = null;
            Destroy(gameObject);
        }

        public void SendToServer(byte[] data)
        {
            if (connection == null)
                return;

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

            switch (messageType)
            {
                case MessageType.Acknowledge:
                    Acknowledge acknowledgedMessage = new Acknowledge(data);
                    TryRemoveHeldMessage(acknowledgedMessage.acknowledgedType, acknowledgedMessage.acknowledgedId);

                    break;
                case MessageType.DisAcknowledge:
                    break;
                case MessageType.Disconnect:
                    onClientDisconnect?.Invoke(new Disconnect(data).id);
                    break;
                case MessageType.Error:
                    break;
                case MessageType.Ping:
                    lastPingTime = Time.time;
                    SendToServer(new Ping(0).Serialize());
                    break;
                case MessageType.AllPings:
                    ClientsPing allClientsPing = new AllPings(data).clientsPing;

                    for (int i = 0; i < allClientsPing.count; i++)
                    {
                        Debug.Log($"[{i}]: {allClientsPing.ms[i]}ms");
                    }

                    break;
                //Moving Cubes message
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponse(new PublicHandshakeResponse(data));
                    SendToServer(Encrypter.Encrypt(ivKeyGenerator.Next(), new PrivateHandshake(id, 0).Serialize()));
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                case MessageType.Position:
                    OnReceiveEvent?.Invoke(data, ip);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator PingTest(float delay)
        {
            yield return new WaitForSeconds(delay);
            SendToServer(new Ping(0).Serialize());
        }

        private void HandleHandshakeResponse(PublicHandshakeResponse data)
        {
            id = data._handshakeData.id;
            seed = data._handshakeData.seed;
            Debug.Log($"Seed: {ClientManager.Instance.Seed}");
            rngGenerator = new Random(seed);
            ivKeyGenerator = new Random(seed);
            OperationsList.Populate(rngGenerator);

            StartCoroutine(SendMessedMessages());
        }

        private IEnumerator SendMessedMessages()
        {
            List<HeldMessage> testMessages = new List<HeldMessage>();
            for (int i = 0; i < 5; i++)
            {
                testMessages.Add(new HeldMessage(i, new ImportantOrderMessage(i).Serialize()));
            }

            _heldMessages.AddRange(testMessages);

            SendToServer(_heldMessages[0].message);
            SendToServer(_heldMessages[2].message);
            SendToServer(_heldMessages[4].message);
            yield return new WaitForSeconds(0.05f);
            SendToServer(_heldMessages[1].message);
            SendToServer(_heldMessages[3].message);
        }

        private bool TryRemoveHeldMessage(MessageType type, int messageId)
        {
            for (int i = 0; i < _heldMessages.Count; i++)
            {
                if (_heldMessages[i].id == messageId && (MessageType)BitConverter.ToInt16(_heldMessages[i].message, MessageOffsets.MessageTypeIndex) == type)
                {
                    _heldMessages.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}