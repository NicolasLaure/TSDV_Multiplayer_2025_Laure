using System.Net;
using Network.Utilities;

namespace Network
{
    public class Client
    {
        public float timeStamp;
        public int id;
        public int elo = 0;
        public string username;
        public bool isInitialized = false;
        public bool isConnectedToServer = false;
        public bool isAuthServer;
        public IPEndPoint ipEndPoint;
        public short ping = 0;
        public float lastActiveTime = 0;
        public float lastPingTime = 0;

        public Client(IPEndPoint ipEndPoint, int id, string username, float timeStamp)
        {
            this.timeStamp = timeStamp;
            this.id = id;
            this.ipEndPoint = ipEndPoint;
            this.username = username;
            ping = 0;
            lastActiveTime = ServerTime.time;
        }
    }
}