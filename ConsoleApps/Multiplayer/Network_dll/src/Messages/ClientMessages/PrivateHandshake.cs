using System;
using System.Collections.Generic;
using Network;
using Network.Enums;

public class PrivateHandshake : Message<(int elo, short color)>
{
    public int elo;
    public short color;

    public PrivateHandshake(int elo, short color)
    {
        isEncrypted = true;
        messageType = MessageType.PrivateHandshake;
        attribs = Attributes.Important | Attributes.Checksum;
        messageId++;
        this.elo = elo;
        this.color = color;
    }

    public PrivateHandshake(byte[] data)
    {
        (elo, color) = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(elo));
        data.AddRange(BitConverter.GetBytes(color));
        return GetFormattedData(data.ToArray());
    }

    public override (int elo, short color) Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        return (BitConverter.ToInt32(payload), BitConverter.ToInt16(payload, sizeof(int)));
    }
}