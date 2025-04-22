using System;
using System.Net;

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

        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        public abstract void OnReceiveData(byte[] data, IPEndPoint ip);

        protected void Update()
        {
            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();
        }
    }
}