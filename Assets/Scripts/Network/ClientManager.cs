using System;
using System.Collections;
using System.Net;
using Network.CheckSum;
using Network.Enums;
using Network.Messages;
using UnityEngine;
using Ping = Network.Messages.Ping;
using Random = System.Random;

namespace Network
{
    public class ClientManager : NetworkManager<ClientManager>
    {
        public Action<short> onPingUpdated;

        private PublicHandshake _heldPublicHandshake;
        private float ping = 0;
        private float lastPingTime;
        private int id;
        private Coroutine handshake;
        private float clientStartTime;

        public Action<int> onClientDisconnect;
        public Action onDisconnection;
        public int Id => id;

        public void StartClient(IPAddress ip, int port)
        {
            this.port = port;
            this.ipAddress = ip;
            lastPingTime = Time.time;
            ping = 0;

            connection = new UdpConnection(ip, port, this);
            clientStartTime = Time.time;
            handshake = StartCoroutine(SendHandshake());
        }

        protected override void Update()
        {
            base.Update();
            ping = Time.time - lastPingTime;
            onPingUpdated?.Invoke((short)(ping * 1000));

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
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, 0);
            switch (messageType)
            {
                case MessageType.Acknowledge:
                    if (handshake != null)
                        StopCoroutine(handshake);

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
                    SendToServer(new PrivateHandshake(id, 0).Serialize());
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

        private IEnumerator SendHandshake()
        {
            HandshakeData handshakeData;
            handshakeData.ip = 0;
            _heldPublicHandshake = new PublicHandshake(handshakeData, 0);
            SendToServer(_heldPublicHandshake.Serialize());

            float timer = 0;
            float startTime = Time.time;
            while (Time.time - clientStartTime < TimeOutTime)
            {
                timer = Time.time - startTime;
                if (timer >= maxResponseWait)
                {
                    SendToServer(_heldPublicHandshake.Serialize());
                    Debug.Log("Resend Handshake");
                    timer = 0;
                    startTime = Time.time;
                }

                yield return null;
            }
        }

        private void HandleHandshakeResponse(PublicHandshakeResponse data)
        {
            id = data._handshakeData.id;
            seed = data._handshakeData.seed;
            Debug.Log($"Seed: {ClientManager.Instance.Seed}");
            rngGenerator = new Random(seed);
            OperationsList.Populate(rngGenerator);
        }
    }
}