using System;
using System.Collections.Generic;
using Network.Enums;

namespace Network.Messages;

public class DeInstantiateRequest : Message<int>
{
    public int instanceId;

    public DeInstantiateRequest(int instanceId)
    {
        messageType = MessageType.DeInstantiateRequest;
        attribs = Attributes.Important;
        this.instanceId = instanceId;
    }

    public DeInstantiateRequest(byte[] data)
    {
        messageType = MessageType.DeInstantiateRequest;
        attribs = Attributes.Important;
        instanceId = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(instanceId));
        return GetFormattedData(data.ToArray());
    }

    public override int Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);

        return (BitConverter.ToInt32(payload));
    }
}