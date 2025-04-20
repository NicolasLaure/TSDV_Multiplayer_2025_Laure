using System;
using System.Net;
using UnityEngine;

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

        public abstract void OnReceiveData(byte[] data, IPEndPoint ip);

        protected void Update()
        {
            // Flush the data in main thread
            if (connection != null)
                connection.FlushReceiveData();
        }
    }
}