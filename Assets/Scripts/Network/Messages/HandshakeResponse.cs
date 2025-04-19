using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;


public struct HandshakeResponseData
{
    public int id;
    public int count;
    public List<Vector3> positions;
}

public class HandshakeResponse : Message<HandshakeResponseData>
{
    public HandshakeResponseData _handshakeData;

    public HandshakeResponse(int id, int count, List<Vector3> positions)
    {
        messageType = MessageType.HandShakeResponse;
        attribs = Attributes.Important;
        _handshakeData.id = id;
        _handshakeData.count = count;
        _handshakeData.positions = positions;
    }

    public HandshakeResponse(byte[] data)
    {
        _handshakeData = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        int size = Marshal.SizeOf(_handshakeData);
        Debug.Log(size);
        byte[] data = new byte[size];

        Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.id), 0, data, 0, sizeof(int));
        Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.count), 0, data, 4, sizeof(int));
        for (int i = 0; i < _handshakeData.count; i++)
        {
            Buffer.BlockCopy(ByteFormat.GetVector3Bytes(_handshakeData.positions[i]), 0, data, 8 + 12 * i, 12);
        }

        return GetFormattedData(data);
    }

    public override HandshakeResponseData Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        HandshakeResponseData data;
        data.id = BitConverter.ToInt32(payload, 0);
        data.count = BitConverter.ToInt32(payload, sizeof(int));
        data.positions = new List<Vector3>();

        for (int i = 0; i < data.count; i++)
        {
            data.positions.Add(ByteFormat.GetVector3FromBytes(payload, sizeof(int) * 2 + i * 12));
        }

        return data;
    }
}