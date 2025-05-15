using System;
using System.Threading;
using Network;

namespace MatchMaker
{
    class Program
    {
        private static MatchmakerManager _matchmakerManager = new MatchmakerManager();

        static void Main(string[] args)
        {
            _matchmakerManager.StartServer(_matchmakerManager.defaultPort);
            _matchmakerManager.ServerLoop();
        }
    }
}