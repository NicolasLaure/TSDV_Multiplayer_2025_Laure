using System;

namespace Network.Utilities
{
    public class Logger
    {
        public static void Log(string text)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(text);
#else
            Console.WriteLine(text);
#endif
        }

        public static void LogError(string text)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(text);
#else
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = previousColor;
#endif
        }
    }
}