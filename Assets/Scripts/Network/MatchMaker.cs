using UnityEngine;
using Logger = Network.Utilities.Logger;

namespace Network
{
    public class MatchMaker : MonoBehaviour
    {
        public MatchmakerManager matchmakerManager;

        private void Awake()
        {
            matchmakerManager = new MatchmakerManager();
            matchmakerManager.StartServer(matchmakerManager.defaultPort);
        }

        private void Update()
        {
            matchmakerManager.Update();
        }
    }
}