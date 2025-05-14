using Network;

namespace MatchMaker
{
    class Program
    {
        private static bool shouldDispose = false;
        private static MatchmakerManager _matchmakerManager = new MatchmakerManager();

        static void Main(string[] args)
        {
            _matchmakerManager.StartServer(_matchmakerManager.defaultPort);
            while (!shouldDispose)
            {
                _matchmakerManager.Update();
            }
        }
    }
}