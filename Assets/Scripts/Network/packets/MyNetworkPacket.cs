using System;
using System.Net;

public class MyNetworkPacket
{
    public PacketType type;
    public short messageStart;
    public short messageEnd;

    public byte[] payload;
    //public float timeStamp;

    public MyNetworkPacket(PacketType type, byte[] data, float timeStamp, int clientId = -1, IPEndPoint ipEndPoint = null)
    {
        this.type = type;
        this.timeStamp = timeStamp;
        this.clientId = clientId;
        this.ipEndPoint = ipEndPoint;
        this.payload = data;

        messageStart = (sizeof(short) * 3);
        messageEnd = messageStart + data.Length;
    }

    public byte[] GetBytes()
    {
        byte[] bytes = new byte[(sizeof(short) * 5) + payload.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(type), 0, bytes, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(messageStart), 0, bytes, 2, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(messageEnd), 0, bytes, 4, 2);
        Buffer.BlockCopy(payload, 0, bytes, messageStart, payload.Length);
        Checksum(bytes, out byte[] first, out byte[] second);
        Buffer.BlockCopy(first, 0, bytes, messageEnd, 2);
        Buffer.BlockCopy(second, 0, bytes, messageEnd + first.Length, 2);
        return bytes;
    }

    public void ChecksumBytes(byte[] data, out byte[] first, out byte[] second)
    {
        CheckSum(data, out int firstNum, out int secondNum);
        first = BitConverter.GetBytes(firstNum);
        second = BitConverter.GetBytes(secondNum);
    }
    public void CheckSum(byte[] data, out short first, out short second)
    {
        first = (data.Length - sizeof(short) * 2) * 32 / 15;
        first = Math.Pow(data.Length - sizeof(short), 5) / 6;
    }
}
