using System.Net;
using Network;
using Network.Enums;
using Network.Messages;
using Network.Messages.Server;
using Utils;

namespace Cubes
{
    public class MovingCubesServer
    {
        private NonAuthoritativeServer serverInstance;

        public void Start()
        {
            NonAuthoritativeServer.onServerStart += Initialize;
        }

        public MovingCubesServer(NonAuthoritativeServer server)
        {
            serverInstance = server;
        }

        protected void Initialize()
        {
            if (serverInstance == null)
                return;

            serverInstance.onNewClient += HandleNewClient;
        }

        private void HandleNewClient(int id)
        {
            ServerHsResponse hsResponse = new ServerHsResponse(id, serverInstance.Seed);
            serverInstance.SendToClient(hsResponse.Serialize(), id);
        }
    }
}