using Cubes;
using Network;
using Network.Utilities;

namespace Server
{
    class Program
    {
        private static bool shouldDispose = false;
        private static NonAuthoritativeServer server = new NonAuthoritativeServer();
        private static MovingCubesServer cubesServer = new MovingCubesServer(server);

        static void Main(string[] args)
        {
            cubesServer.Start();
            server.Start(int.Parse(args[0]));
            Logger.Log($"Started Server on Port {server.port}");
            while (!shouldDispose)
            {
                server.Update();
            }
        }
    }
}