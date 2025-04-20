using System;
using UnityEngine;

public enum MessageType : short
{
    HandShake = -1,
    HandShakeResponse,
    Acknowledge, //To Do
    DisAcknowledge, //To Do
    Disconnect, //To Do
    Ping, //To Do
    Position,
    Console,
    Error,
}

[Flags]
public enum Attributes : short
{
    None = 1 << 0,
    Important = 1 << 2,
    Checksum = 1 << 3,
    Critical = 1 << 4,
    Order = 1 << 5
}

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

            if (attribs == Attributes.Checksum)
                tailSize = sizeof(int) * 2;
        }

        byte[] header = new byte[headerSize];
        byte[] tail = tailSize == 0 ? null : new byte[tailSize];
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

        Buffer.BlockCopy(header, 0, data, 0, header.Length);
        Buffer.BlockCopy(input, 0, data, messageStart, input.Length);

        if (tail == null) return data;

        ChecksumBytes(data, out byte[] first, out byte[] second);
        Buffer.BlockCopy(first, 0, tail, 0, sizeof(int));
        Buffer.BlockCopy(second, 0, tail, 0 + first.Length, sizeof(int));
        Buffer.BlockCopy(tail, 0, data, messageEnd, tail.Length);

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

    public void ChecksumBytes(byte[] data, out byte[] first, out byte[] second)
    {
        CheckSum(data, out short firstNum, out short secondNum);
        first = BitConverter.GetBytes(firstNum);
        second = BitConverter.GetBytes(secondNum);
    }

    public void CheckSum(byte[] data, out short first, out short second)
    {
        first = (short)((data.Length - sizeof(short) * 2) * 32 / 15);
        second = (short)(Math.Pow(data.Length - sizeof(short), 5) / 6);
    }
}