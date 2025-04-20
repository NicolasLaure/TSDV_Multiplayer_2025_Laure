using System;
using UnityEngine;

public class Acknowledge : Message<(MessageType, int)>
{
    private MessageType acknowledgedType;
    private int acknowledgedId;

    public Acknowledge(MessageType type, int messageId)
    {
        messageType = MessageType.Acknowledge;
        attribs = Attributes.None;
        this.messageId = messageId;

        acknowledgedType = type;
        acknowledgedId = messageId;
    }

    public Acknowledge(byte[] data)
    {
        messageType = MessageType.Acknowledge;
        attribs = Attributes.None;
        (MessageType, int) deserializedData = Deserialize(data);
        acknowledgedType = deserializedData.Item1;
        acknowledgedId = deserializedData.Item2;
    }

    public override byte[] Serialize()
    {
        byte[] payload = new byte[sizeof(short) + sizeof(int)];
        Buffer.BlockCopy(BitConverter.GetBytes((short)acknowledgedType), 0, payload, 0, sizeof(short));
        Buffer.BlockCopy(BitConverter.GetBytes(acknowledgedId), 0, payload, 2, sizeof(int));
        return GetFormattedData(payload);
    }

    public override (MessageType, int) Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        MessageType receivedType = (MessageType)BitConverter.ToInt16(payload, 0);
        int receivedId = BitConverter.ToInt32(payload, 2);

        return (receivedType, receivedId);
    }
}