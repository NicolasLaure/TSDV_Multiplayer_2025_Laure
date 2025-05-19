using System;

namespace Network
{
    public class ClientManager : MonoBehaviourSingleton<ClientManager>
    {
        public NetworkClient networkClient;

        public bool isServerActive = false;

        protected override void Initialize()
        {
            networkClient = new NetworkClient();
        }

        private void Update()
        {
            if (isServerActive)
            {
                networkClient.Update();
            }
        }

        private void OnDestroy()
        {
            networkClient.EndClient();
            if (Instance == this)
                Instance = null;
        }
    }
}