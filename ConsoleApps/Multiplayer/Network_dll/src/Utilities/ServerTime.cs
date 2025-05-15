using System;

namespace Network.Utilities
{
    public class ServerTime
    {
        public static DateTime startTime;

        public ServerTime()
        {
            startTime = DateTime.UtcNow;
        }

        public static float time => ((float)((DateTime.UtcNow - startTime).TotalMilliseconds / 1000));
    }
}