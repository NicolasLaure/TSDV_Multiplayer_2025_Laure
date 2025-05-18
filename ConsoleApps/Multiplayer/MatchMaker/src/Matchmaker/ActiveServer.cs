using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace Network.Matchmaker
{
    public class ActiveServer
    {
        public List<Client> clients;
        public IPAddress serverIp;
        public int serverPort;
        public Process process;

        public ActiveServer(List<Client> clients, IPAddress serverIp, int port, Process serverProcess)
        {
            this.clients = clients;
            this.serverIp = serverIp;
            serverPort = port;
            process = serverProcess;
        }
    }
}