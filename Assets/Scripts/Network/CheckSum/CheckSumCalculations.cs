using System;
using System.Collections.Generic;
using Network.Messages;
using UnityEngine;

namespace Network.CheckSum
{
    public class CheckSumCalculations
    {
        static int MessageEndIndex => (sizeof(bool) + sizeof(short) * 3 + sizeof(int));

        public static void ChecksumBytes(byte[] data, out byte[] resultBytes, List<BitOperations> operationsList)
        {
            CheckSum(data, out int firstNum, operationsList);
            resultBytes = BitConverter.GetBytes(firstNum);
        }

        public static void CheckSum(byte[] data, out int result, List<BitOperations> operationsList)
        {
            Span<int> dataAsInts = GetIntsFromData(data);
            result = OperateThrough(dataAsInts, operationsList);
        }

        private static int[] GetIntsFromData(byte[] data)
        {
            int quantityOfInts = (int)Math.Ceiling((double)data.Length / 4);
            int[] ints = new int[quantityOfInts];

            for (int i = 0; i < quantityOfInts; i++)
            {
                if (i * 4 + 3 < data.Length)
                {
                    ints[i] = BitConverter.ToInt32(data, i * 4);
                }
                else
                {
                    int missingBytesQty = sizeof(int) - data.Length % sizeof(int) * sizeof(int);
                    byte[] missingBytes = new byte[sizeof(int)];
                    for (int j = 0; j < missingBytesQty; j++)
                    {
                        missingBytes[j] = new byte();
                    }

                    for (int j = 0; j < sizeof(int) - missingBytesQty; j++)
                    {
                        missingBytes[j] = data[i + j];
                    }

                    ints[i] = BitConverter.ToInt32(missingBytes);
                }
            }

            return ints;
        }

        private static int OperateThrough(Span<int> dataAsInts, List<BitOperations> operationsList)
        {
            int index = 0;
            while (dataAsInts.Length > 1)
            {
                if (index >= operationsList.Count)
                    index = 0;

                switch (operationsList[index])
                {
                    case BitOperations.RightShift:
                        for (int i = 0; i < dataAsInts.Length; i++)
                        {
                            dataAsInts[i] >>= 1;
                        }

                        break;
                    case BitOperations.LeftShift:
                        for (int i = 0; i < dataAsInts.Length; i++)
                        {
                            dataAsInts[i] <<= 1;
                        }

                        break;
                    case BitOperations.And:
                        dataAsInts[1] &= dataAsInts[0];
                        dataAsInts = dataAsInts.Slice(1, dataAsInts.Length - 1);
                        break;
                    case BitOperations.Or:
                        dataAsInts[1] |= dataAsInts[0];
                        dataAsInts = dataAsInts.Slice(1, dataAsInts.Length - 1);
                        break;
                    case BitOperations.Xor:
                        dataAsInts[1] ^= dataAsInts[0];
                        dataAsInts = dataAsInts.Slice(1, dataAsInts.Length - 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                index++;
            }

            return dataAsInts[0];
        }

        public static bool IsCheckSumOk(byte[] data)
        {
            int messageEnd = BitConverter.ToInt16(data, MessageOffsets.EndIndex);
            int checkSum1 = BitConverter.ToInt32(data, messageEnd);
            int checkSum2 = BitConverter.ToInt32(data, messageEnd + sizeof(int));
            Span<byte> dataSpan = data;

            Span<byte> checkSum1Span = dataSpan.Slice(0, messageEnd);
            Span<byte> checkSum2Span = dataSpan.Slice(0, messageEnd + sizeof(int));
            CheckSum(checkSum1Span.ToArray(), out int res1, OperationsList.OperationsCheckSum1);
            Debug.Log($"Checksum1 = {checkSum1}, res1 = {res1}");
            CheckSum(checkSum2Span.ToArray(), out int res2, OperationsList.OperationsCheckSum2);
            Debug.Log($"Checksum2 = {checkSum2}, res2 = {res2}");
            return res1 == checkSum1 && res2 == checkSum2;
        }
    }
}