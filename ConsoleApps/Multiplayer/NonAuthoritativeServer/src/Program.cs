using Network;
using Network.Utilities;

namespace Server
{
    class Program
    {
        private static bool shouldDispose = false;
        private static NonAuthoritativeServer server = new NonAuthoritativeServer();

        static void Main(string[] args)
        {
            server.StartServer(int.Parse(args[0]));
            Logger.Log($"Started Server on Port {server.port}");
            while (!shouldDispose)
            {
                server.Update();
            }
        }
    }
}