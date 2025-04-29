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

        private Handshake heldHandshake;
        private short ping = 0;
        private int id;
        private Coroutine handshake;
        private float clientStartTime;

        public Action<int> onClientDisconnect;
        public int Id => id;

        public void StartClient(IPAddress ip, int port)
        {
            this.port = port;
            this.ipAddress = ip;

            connection = new UdpConnection(ip, port, this);
            clientStartTime = Time.time;
            handshake = StartCoroutine(SendHandshake());
        }

        public void EndClient(int instanceID)
        {
            SendToServer(new Disconnect(instanceID).Serialize());
            connection = null;
        }

        public void SendToServer(byte[] data)
        {
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

                    ping = new Ping(data).ms;
                    SendToServer(new Ping(0).Serialize());
                    onPingUpdated?.Invoke(ping);
                    break;

                //Moving Cubes message
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponse(new HandshakeResponse(data));
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
            heldHandshake = new Handshake(handshakeData, 0);
            SendToServer(heldHandshake.Serialize());

            float timer = 0;
            float startTime = Time.time;
            while (Time.time - clientStartTime < TimeOutTime)
            {
                timer = Time.time - startTime;
                if (timer >= maxResponseWait)
                {
                    SendToServer(heldHandshake.Serialize());
                    Debug.Log("Resend Handshake");
                    timer = 0;
                    startTime = Time.time;
                }

                yield return null;
            }
        }

        private void HandleHandshakeResponse(HandshakeResponse data)
        {
            id = data._handshakeData.id;
            seed = data._handshakeData.seed;
            Debug.Log($"Seed: {ClientManager.Instance.Seed}");
            rngGenerator = new Random(seed);
            OperationsList.Populate(rngGenerator);
        }
    }
}