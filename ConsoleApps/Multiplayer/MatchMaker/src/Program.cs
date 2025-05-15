using System;
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
            Console.WriteLine($"MathchMaker initialized on port {_matchmakerManager.port}");
            while (!shouldDispose)
            {
                _matchmakerManager.Update();
            }
        }
    }
}