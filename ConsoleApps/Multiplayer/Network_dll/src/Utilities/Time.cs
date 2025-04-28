using System;

namespace Network.Utilities
{
    public static class Time
    {
        public static float deltaTime;
        public static float timeSinceStartup;

        private static DateTime startupTime;

        static Time()
        {
            startupTime = DateTime.Now;
        }
    }
}