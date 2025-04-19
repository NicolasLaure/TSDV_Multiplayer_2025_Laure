using System;
using UnityEngine;

public class Ping : Message<short>
{
    public short ms;

    public Ping(short ms)
    {
        messageType = MessageType.Ping;
        attribs = Attributes.None;
        this.ms = ms;
    }

    public Ping(byte[] data)
    {
        ms = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        return GetFormattedData(BitConverter.GetBytes(ms));
    }

    public override short Deserialize(byte[] message)
    {
        return BitConverter.ToInt16(ExtractPayload(message));
    }
}