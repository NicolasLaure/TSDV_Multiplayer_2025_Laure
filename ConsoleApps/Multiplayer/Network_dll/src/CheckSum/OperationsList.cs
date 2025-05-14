using System.Collections.Generic;
using Random = System.Random;

namespace Network.CheckSum
{
    public static class OperationsList
    {
        public static readonly int OperationsCount = 10;
        public static List<BitOperations> OperationsCheckSum1 = new List<BitOperations>();
        public static List<BitOperations> OperationsCheckSum2 = new List<BitOperations>();

        public static void Populate(Random rngGenerator)
        {
            for (int i = 0; i < OperationsCount; i++)
            {
                OperationsCheckSum1.Add((BitOperations)rngGenerator.Next(0, (int)BitOperations.Count));
                OperationsCheckSum2.Add((BitOperations)rngGenerator.Next(0, (int)BitOperations.Count));
            }
        }
    }
}