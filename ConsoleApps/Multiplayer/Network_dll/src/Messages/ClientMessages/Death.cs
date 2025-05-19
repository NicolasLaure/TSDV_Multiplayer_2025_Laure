using System;
using Network;
using Network.Enums;

namespace Network_dll.Messages.ClientMessages;

public class Death : Message<int>
{
    public int deadId;

    public Death(int id)
    {
        messageType = MessageType.Death;
        attribs = Attributes.None;
        deadId = id;
    }

    public Death(byte[] data)
    {
        deadId = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        return GetFormattedData(BitConverter.GetBytes(deadId));
    }

    public override int Deserialize(byte[] message)
    {
        return BitConverter.ToInt32(ExtractPayload(message));
    }
}