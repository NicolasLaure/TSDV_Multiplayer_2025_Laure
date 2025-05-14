using System.Collections.Generic;
using System.Net;

namespace Network.Matchmaker
{
    public class ActiveServer
    {
        public List<int> clientIds;
        public IPAddress serverIp;
        public int serverPort;

        public ActiveServer(List<int> clients, IPAddress serverIp, int port)
        {
            this.clientIds = clients;
            this.serverIp = serverIp;
            serverPort = port;
        }
    }
}