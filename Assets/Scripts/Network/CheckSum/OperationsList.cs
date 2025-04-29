using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Network.CheckSum
{
    public static class OperationsList
    {
        public static readonly int OperationsCount = 10;
        public static List<BitOperations> Operations = new List<BitOperations>();

        public static void Populate(Random rngGenerator)
        {
            for (int i = 0; i < OperationsCount; i++)
            {
                Operations.Add((BitOperations)rngGenerator.Next(0, (int)BitOperations.Count));
                Debug.Log($"{i}: {(int)Operations[i]}");
            }
        }
    }
}