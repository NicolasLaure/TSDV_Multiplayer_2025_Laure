using System;
using Network;
using Network.Enums;
using Network.Messages;

namespace Messages.ClientMessages;

public class Crouch : Message<bool>
{
    public bool isCrouching;

    public Crouch(bool isCrouching, int instanceId)
    {
        messageType = MessageType.Crouch;
        attribs = Attributes.Important;
        this.isCrouching = isCrouching;
        clientId = instanceId;
        messageId++;
    }

    public Crouch(byte[] data)
    {
        messageType = MessageType.Crouch;
        attribs = Attributes.Important;
        isCrouching = Deserialize(data);
        clientId = BitConverter.ToInt32(data, MessageOffsets.ClientIdIndex);
        messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
    }


    public override byte[] Serialize()
    {
        byte[] data = BitConverter.GetBytes(isCrouching);
        return GetFormattedData(data);
    }

    public override bool Deserialize(byte[] message)
    {
        return BitConverter.ToBoolean(ExtractPayload(message));
    }
}