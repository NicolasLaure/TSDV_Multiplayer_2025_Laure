using System;
using UnityEngine;

public class Position : Message<(Vector3, int)>
{
    public Vector3 pos;
    public int instanceID;

    public Position(Vector3 pos, int instanceID, int messageId)
    {
        messageType = MessageType.Position;
        attribs = Attributes.Order;
        this.pos = pos;
        this.instanceID = instanceID;
        this.messageId = messageId;
    }

    public Position(byte[] data)
    {
        (Vector3, int) posAndIndex = Deserialize(data);
        pos = posAndIndex.Item1;
        instanceID = posAndIndex.Item2;
    }

    public override byte[] Serialize()
    {
        byte[] data = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(instanceID), 0, data, 0, 4);
        Buffer.BlockCopy(ByteFormat.GetVector3Bytes(pos), 0, data, 4, 12);

        return GetFormattedData(data);
    }

    public override (Vector3, int) Deserialize(byte[] message)
    {
        byte[] data = ExtractPayload(message);
        int index = BitConverter.ToInt32(data, 0);
        byte[] componentsData = new byte[12];
        Buffer.BlockCopy(data, 4, componentsData, 0, 12);
        return (ByteFormat.GetVector3FromBytes(componentsData, 0), index);
    }
}