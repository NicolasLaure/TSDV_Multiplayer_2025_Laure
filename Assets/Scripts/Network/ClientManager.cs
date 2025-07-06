using MidTerm2;

namespace Network
{
    public class ClientManager : MonoBehaviourSingleton<ClientManager>
    {
        public ReflectiveClient<CastlesModel> networkClient;

        public bool isServerActive = false;

        protected override void Initialize()
        {
            networkClient = new ReflectiveClient<CastlesModel>();
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