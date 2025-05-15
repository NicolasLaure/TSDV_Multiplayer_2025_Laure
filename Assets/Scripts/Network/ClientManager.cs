using System;

namespace Network
{
    public class ClientManager : MonoBehaviourSingleton<ClientManager>
    {
        public NetworkClient networkClient;

        protected override void Initialize()
        {
            networkClient = new NetworkClient();
        }

        private void Update()
        {
            networkClient.Update();
        }

        private void OnDestroy()
        {
            networkClient.EndClient();
        }
    }
}