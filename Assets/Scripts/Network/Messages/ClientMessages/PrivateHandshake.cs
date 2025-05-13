using System;
using Network;
using Network.Enums;

public class PrivateHandshake : Message<int>
{
    public int elo;

    public PrivateHandshake(int elo, int messageId)
    {
        isEncrypted = true;
        messageType = MessageType.PrivateHandshake;
        attribs = Attributes.Important | Attributes.Checksum;
        this.messageId = messageId;
        this.elo = elo;
    }

    public PrivateHandshake(byte[] data)
    {
        elo = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        return GetFormattedData(BitConverter.GetBytes(elo));
    }

    public override int Deserialize(byte[] message)
    {
        return BitConverter.ToInt32(ExtractPayload(message));
    }
}