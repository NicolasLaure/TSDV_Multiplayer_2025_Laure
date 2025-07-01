using UnityEngine;

namespace Utils
{
    public static class Route
    {
        public static string RouteString(int[] route)
        {
            string routeString = "";
            for (int i = 0; i < route.Length; i++)
            {
                routeString += $"[{route[i]}]";
            }

            return routeString;
        }
    }
}