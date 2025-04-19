using System;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

public struct HandshakeData
{
    public int ip;
}

public class Handshake : Message<HandshakeData>
{
    private HandshakeData _handshakeData;

    public Handshake(HandshakeData handshakeData)
    {
        messageType = MessageType.HandShake;
        attribs = Attributes.Important;
        _handshakeData = handshakeData;
    }

    public Handshake(byte[] data)
    {
        _handshakeData = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        int size = Marshal.SizeOf(_handshakeData);
        Debug.Log(size);
        byte[] data = new byte[size];

        Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.ip), 0, data, 0, sizeof(int));
        return GetFormattedData(data);
    }

    public override HandshakeData Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        HandshakeData data;
        data.ip = BitConverter.ToInt32(payload, 0);
        return data;
    }
}