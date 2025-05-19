using UnityEngine;

namespace Utils
{
    public class LoggerHandler : MonoBehaviour
    {
        void Start()
        {
            Network.Utilities.Logger.onLog += Log;
            Network.Utilities.Logger.onLogError += LogErr;
        }

        void Log(string text)
        {
            Debug.Log(text);
        }

        void LogErr(string text)
        {
            Debug.LogError(text);
        }
    }
}