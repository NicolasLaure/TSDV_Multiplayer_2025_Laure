using System;
using System.Collections.Generic;
using Network.Messages;
using Network.Utilities;

namespace Network.CheckSum
{
    public class CheckSumCalculations
    {
        static int MessageEndIndex => (sizeof(bool) + sizeof(short) * 3 + sizeof(int));

        private static int circleShiftOffset => (sizeof(uint) * 8 - 1);

        public static void ChecksumBytes(byte[] data, out byte[] resultBytes, List<BitOperations> operationsList)
        {
            CheckSum(data, out uint firstNum, operationsList);
            resultBytes = BitConverter.GetBytes(firstNum);
        }

        public static void CheckSum(byte[] data, out uint result, List<BitOperations> operationsList)
        {
            Span<uint> dataAsInts = GetIntsFromData(data);
            result = OperateThrough(dataAsInts, operationsList);
        }

        private static uint[] GetIntsFromData(byte[] data)
        {
            int quantityOfInts = (int)Math.Ceiling((double)data.Length / 4);
            List<uint> ints = new List<uint>();

            for (int i = 0; i < quantityOfInts; i++)
            {
                if (i * 4 + 3 < data.Length)
                {
                    ints.Add(BitConverter.ToUInt32(data, i * 4));
                }
                else
                {
                    float remainder = (float)(data.Length % sizeof(int)) / sizeof(int);
                    int missingBytesQty = sizeof(int) - (int)(remainder * sizeof(int));
                    byte[] missingBytes = new byte[sizeof(int)];
                    for (int j = 0; j < sizeof(int); j++)
                    {
                        missingBytes[j] = new byte();
                    }

                    for (int j = 0; j < sizeof(int) - missingBytesQty; j++)
                    {
                        missingBytes[j] = data[(i * 4) + j];
                    }

                    ints.Add(BitConverter.ToUInt32(missingBytes));
                }
            }

            return ints.ToArray();
        }

        private static uint OperateThrough(Span<uint> dataAsInts, List<BitOperations> operationsList)
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
                            dataAsInts[i] = dataAsInts[i] >> 1 | dataAsInts[i] << circleShiftOffset;
                        }

                        break;
                    case BitOperations.LeftShift:
                        for (int i = 0; i < dataAsInts.Length; i++)
                        {
                            dataAsInts[i] = dataAsInts[i] << 1 | dataAsInts[i] >> circleShiftOffset;
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
            uint checkSum1 = BitConverter.ToUInt32(data, messageEnd);
            uint checkSum2 = BitConverter.ToUInt32(data, messageEnd + sizeof(uint));

            Span<byte> checkSum1Span = data[..messageEnd];
            Span<byte> checkSum2Span = data[..(messageEnd + sizeof(int))];
            CheckSum(checkSum1Span.ToArray(), out uint res1, OperationsList.OperationsCheckSum1);
            Logger.Log($"Checksum1 = {checkSum1}, res1 = {res1}");
            CheckSum(checkSum2Span.ToArray(), out uint res2, OperationsList.OperationsCheckSum2);
            Logger.Log($"Checksum2 = {checkSum2}, res2 = {res2}");
            return res1 == checkSum1 && res2 == checkSum2;
        }
    }
}