using System;

namespace Network.Utilities
{
    public class Logger
    {
        public static void Log(string text)
        {
            onLog?.Invoke(text);
            Console.WriteLine(text);
        }

        public static void LogError(string text)
        {
            onLogError?.Invoke(text);
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = previousColor;
        }

        public static Action<string> onLog;
        public static Action<string> onLogError;
    }
}