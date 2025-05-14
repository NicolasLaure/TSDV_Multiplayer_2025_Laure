using System;
using System.Diagnostics;

namespace Network.Utilities
{
    public class Time
    {
        public static DateTime startTime;

        Time()
        {
            startTime = DateTime.UtcNow;
        }

        public static float time => ((float)((DateTime.UtcNow - startTime).TotalMilliseconds / 1000));
    }
}