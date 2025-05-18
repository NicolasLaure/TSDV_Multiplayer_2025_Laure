using System;
using System.Collections.Generic;
using Network;
using Network.Enums;
using Network.Messages;

namespace Messages.ClientMessages;

public class Crouch : Message<(bool isCrouching, int instanceId)>
{
    public bool isCrouching;
    public int instanceId;

    public Crouch(bool isCrouching, int instanceId)
    {
        messageType = MessageType.Crouch;
        attribs = Attributes.Important;
        this.isCrouching = isCrouching;
        this.instanceId = instanceId;
        messageId++;
    }

    public Crouch(byte[] data)
    {
        messageType = MessageType.Crouch;
        attribs = Attributes.Important;
        (isCrouching, instanceId) = Deserialize(data);
        clientId = BitConverter.ToInt32(data, MessageOffsets.ClientIdIndex);
        messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
    }
    
    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(isCrouching));
        data.AddRange(BitConverter.GetBytes(instanceId));
        return GetFormattedData(data.ToArray());
    }

    public override (bool isCrouching, int instanceId) Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        return (BitConverter.ToBoolean(payload), BitConverter.ToInt32(payload, sizeof(bool)));
    }
}