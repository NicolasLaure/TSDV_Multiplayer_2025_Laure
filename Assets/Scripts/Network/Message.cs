using System;
using System.Collections.Generic;
using Network.CheckSum;
using Network.Enums;
using UnityEngine;

namespace Network
{
    public abstract class Message<T>
    {
        public MessageType messageType;
        public Attributes attribs;

        public short messageStart;
        public short messageEnd;

        public int messageId = 0;

        public byte[] GetFormattedData(byte[] input)
        {
            int headerSize = sizeof(short) * 2;
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

            int offset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes((short)messageType), 0, header, offset, sizeof(short));
            offset += sizeof(short);
            Buffer.BlockCopy(BitConverter.GetBytes((short)attribs), 0, header, offset, sizeof(short));
            offset += sizeof(short);
            if (messageType != MessageType.Ping)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(messageId), 0, header, offset, sizeof(int));
                offset += sizeof(int);

                Buffer.BlockCopy(BitConverter.GetBytes(messageStart), 0, header, offset, sizeof(short));
                offset += sizeof(short);
                Buffer.BlockCopy(BitConverter.GetBytes(messageEnd), 0, header, offset, sizeof(short));
            }

            Buffer.BlockCopy(header, 0, data, 0, headerSize);
            Buffer.BlockCopy(input, 0, data, messageStart, input.Length);

            if (tailSize == 0) return data;


            Span<byte> dataSpan = data;
            Span<byte> checkSum1Span = dataSpan.Slice(0, messageEnd);
            Span<byte> checkSum2Span = dataSpan.Slice(0, messageEnd + sizeof(int));
            
            CheckSumCalculations.ChecksumBytes(checkSum1Span.ToArray(), out byte[] first, OperationsList.OperationsCheckSum1);
            Buffer.BlockCopy(first, 0, data, messageEnd, sizeof(int));
            CheckSumCalculations.ChecksumBytes(checkSum2Span.ToArray(), out byte[] second, OperationsList.OperationsCheckSum2);
            Buffer.BlockCopy(second, 0, data, messageEnd + first.Length, sizeof(int));

            return data;
        }

        public static byte[] ExtractPayload(byte[] message)
        {
            byte[] payload;
            int payloadSize;
            int offset = 0;

            MessageType type = (MessageType)BitConverter.ToInt16(message, offset);
            offset += sizeof(short);
            Attributes messageAttributes = (Attributes)BitConverter.ToInt16(message, offset);
            offset += sizeof(short);

            if (type == MessageType.Ping)
            {
                payloadSize = message.Length - sizeof(short) * 2;
                payload = new byte[payloadSize];
                Buffer.BlockCopy(message, sizeof(short) * 2, payload, 0, payloadSize);
                return payload;
            }

            offset += sizeof(int);
            short messageStart = BitConverter.ToInt16(message, offset);
            offset += sizeof(short);
            short messageEnd = BitConverter.ToInt16(message, offset);

            payloadSize = messageEnd - messageStart;
            payload = new byte[payloadSize];

            Buffer.BlockCopy(message, messageStart, payload, 0, payloadSize);
            return payload;
        }

        public abstract byte[] Serialize();
        public abstract T Deserialize(byte[] message);
    }
}