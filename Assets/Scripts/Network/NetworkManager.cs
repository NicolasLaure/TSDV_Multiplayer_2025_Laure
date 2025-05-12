using System;
using System.Collections.Generic;
using System.Net;
using Network.SaveStructures;

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

        public abstract void OnReceiveData(byte[] data, IPEndPoint ip);

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
    }
}