using System;
using System.Collections.Generic;
using Network.CheckSum;
using Network.Enums;
using Network.Messages;
using UnityEngine;

namespace Network
{
    public abstract class Message<T>
    {
        public bool isEncrypted = false;
        public MessageType messageType;
        public Attributes attribs;

        public int messageId = 0;
        public short messageStart;
        public short messageEnd;

        public byte[] GetFormattedData(byte[] input)
        {
            int headerSize = sizeof(bool) + sizeof(short) * 2;
            int tailSize = 0;
            if (messageType != MessageType.Ping)
            {
                headerSize += sizeof(short) * 2 + sizeof(int);

                if (attribs.HasFlag(Attributes.Checksum))
                    tailSize = sizeof(int) * 2;
            }

            byte[] header = new byte[headerSize];
            byte[] data = new byte[headerSize + tailSize + input.Length];

            messageStart = (short)headerSize;
            messageEnd = (short)(headerSize + input.Length);

            Buffer.BlockCopy(BitConverter.GetBytes(isEncrypted), 0, header, MessageOffsets.IsEncryptedIndex, sizeof(bool));
            Buffer.BlockCopy(BitConverter.GetBytes((short)messageType), 0, header, MessageOffsets.MessageTypeIndex, sizeof(short));
            Buffer.BlockCopy(BitConverter.GetBytes((short)attribs), 0, header, MessageOffsets.AttribsIndex, sizeof(short));
            if (messageType != MessageType.Ping)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(messageId), 0, header, MessageOffsets.IdIndex, sizeof(int));

                Buffer.BlockCopy(BitConverter.GetBytes(messageStart), 0, header, MessageOffsets.StartIndex, sizeof(short));
                Buffer.BlockCopy(BitConverter.GetBytes(messageEnd), 0, header, MessageOffsets.EndIndex, sizeof(short));
            }

            Buffer.BlockCopy(header, 0, data, 0, headerSize);
            Buffer.BlockCopy(input, 0, data, messageStart, input.Length);

            if (tailSize == 0) return data;

            CheckSumCalculations.ChecksumBytes(data[..messageEnd], out byte[] first, OperationsList.OperationsCheckSum1);
            Buffer.BlockCopy(first, 0, data, messageEnd, sizeof(int));
            CheckSumCalculations.ChecksumBytes(data[..(messageEnd + sizeof(int))], out byte[] second, OperationsList.OperationsCheckSum2);
            Buffer.BlockCopy(second, 0, data, messageEnd + first.Length, sizeof(int));

            return data;
        }

        public static byte[] ExtractPayload(byte[] message)
        {
            byte[] payload;
            int payloadSize;

            MessageType type = (MessageType)BitConverter.ToInt16(message, MessageOffsets.MessageTypeIndex);
            Attributes messageAttributes = (Attributes)BitConverter.ToInt16(message, MessageOffsets.AttribsIndex);

            if (type == MessageType.Ping)
            {
                payloadSize = message.Length - sizeof(short) * 2 - sizeof(bool);
                payload = new byte[payloadSize];
                Buffer.BlockCopy(message, sizeof(bool) + sizeof(short) * 2, payload, 0, payloadSize);
                return payload;
            }

            short messageStart = BitConverter.ToInt16(message, MessageOffsets.StartIndex);
            short messageEnd = BitConverter.ToInt16(message, MessageOffsets.EndIndex);

            payloadSize = messageEnd - messageStart;
            payload = new byte[payloadSize];

            Buffer.BlockCopy(message, messageStart, payload, 0, payloadSize);
            return payload;
        }

        public abstract byte[] Serialize();
        public abstract T Deserialize(byte[] message);
    }
}