using System;
using System.Collections.Generic;
using System.Net;
using Network.CheckSum;
using Network.Encryption;
using Network.Enums;
using Network.Messages;
using Network.SaveStructures;
using UnityEngine;
using Random = System.Random;

namespace Network
{
    public abstract class NetworkManager<T> : MonoBehaviourSingleton<T>, IReceiveData where T : NetworkManager<T>
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
        protected int defaultPort = 60325;

        public int Seed => seed;

        public abstract void OnReceiveData(byte[] inputdata, IPEndPoint ip);

        protected virtual void Update()
        {
            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();
        }

        protected void SaveCriticalMessage(int clientId, int messageId, byte[] message)
        {
            criticalMessages.Add(new CriticalMessage(clientId, messageId, message));
        }

        protected bool CheckHeader(int IvKey, out MessageType type, out Attributes attributes, byte[] rawData, out byte[] data)
        {
            data = rawData;
            if (BitConverter.ToBoolean(rawData, 0))
                data = Encrypter.Decrypt(IvKey, rawData);

            type = (MessageType)BitConverter.ToInt16(rawData, MessageOffsets.MessageTypeIndex);
            attributes = (Attributes)BitConverter.ToInt16(rawData, MessageOffsets.AttribsIndex);

            if (attributes.HasFlag(Attributes.Checksum))
            {
                if (!CheckSumCalculations.IsCheckSumOk(rawData))
                {
                    Debug.Log("CheckSum Not Okay");
                    return false;
                }

                Debug.Log("CheckSum Okay");
            }

            return true;
        }
    }
}