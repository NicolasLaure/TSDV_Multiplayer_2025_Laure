using System;
using System.IO;
using System.Net;

public enum PacketType : short
{
    HandShake,
    Acknowledge, //To Do
    DisAcknowledge, //To Do
    Disconnect, //To Do
    Error,
    Ping, //To Do
    Pong,
    Message,
}
public class MyNetworkPacket
{
    public PacketType type;
    public Attributes attribs;

    public short messageStart;
    public short messageEnd;

    public byte[] payload;
    public float? timeStamp;

    public int? cliendId;
    public IPEndPoint ipEndPoint;

    #region Constructors

    public MyNetworkPacket(PacketType type, Attributes attribs, byte[] data)
    {
        this.type = type;
        this.timeStamp = null;
        this.cliendId = null;
        this.ipEndPoint = null;
        this.payload = data;

        this.attribs = attribs;
        messageStart = (sizeof(short) * 4);
        messageEnd = (short)(messageStart + data.Length);
    }

    public MyNetworkPacket(PacketType type, Attributes attribs, byte[] data, float timeStamp, int clientId = -1)
    {
        this.type = type;
        this.attribs = attribs;
        this.timeStamp = timeStamp;
        this.cliendId = clientId;
        this.ipEndPoint = null;
        this.payload = data;

        messageStart = (sizeof(short) * 3);
        messageEnd = (short)(messageStart + data.Length);
    }

    #endregion

    public byte[] GetBytes()
    {
        int extraLength = sizeof(short) * 2;
        if (type != PacketType.Ping || type != PacketType.Pong)
        {
            extraLength += sizeof(short) * 2;
            if (attribs == Attributes.Checksum)
                extraLength += sizeof(short) * 2;
        }

        byte[] bytes = new byte[payload.Length + extraLength];
        Buffer.BlockCopy(BitConverter.GetBytes((short)type), 0, bytes, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)attribs), 0, bytes, 2, 2);
        int offset = sizeof(short) * 2;
        if (type != PacketType.Ping || type != PacketType.Pong)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(messageStart), 0, bytes, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(messageEnd), 0, bytes, 6, 2);
            offset += sizeof(short) * 2;
        }

        Buffer.BlockCopy(payload, 0, bytes, offset, payload.Length);
        if (attribs == Attributes.Checksum)
        {
            ChecksumBytes(bytes, out byte[] first, out byte[] second);
            Buffer.BlockCopy(first, 0, bytes, messageEnd, 2);
            Buffer.BlockCopy(second, 0, bytes, messageEnd + first.Length, 2);
        }

        return bytes;
    }

    public static MyNetworkPacket GetPacket(byte[] data)
    {
        PacketType packetType = (PacketType)System.BitConverter.ToInt16(data, 0);
        Attributes attributes = (Attributes)System.BitConverter.ToInt16(data, 2);
        int msgStart = sizeof(short) * 2;
        int payloadLength = data.Length - sizeof(short) * 2;

        if (packetType != PacketType.Ping || packetType != PacketType.Pong)
        {
            payloadLength -= sizeof(short) * 2;
            msgStart += sizeof(short) * 2;
            if (attributes == Attributes.Checksum)
                payloadLength -= sizeof(short) * 2;
        }

        byte[] packetPayload = new byte[payloadLength];

        Buffer.BlockCopy(data, msgStart, packetPayload, 0, payloadLength);
        return new MyNetworkPacket(packetType, attributes, packetPayload);
    }

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