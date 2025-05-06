using System;
using Network;
using Network.Enums;
using UnityEngine;

public class AllPings : Message<ClientsPing>
{
    public ClientsPing clientsPing;

    private int DataSize => sizeof(int) + clientsPing.count * sizeof(short);

    public AllPings(short[] ms, int count)
    {
        messageType = MessageType.AllPings;
        attribs = Attributes.None;
        clientsPing.ms = ms;
        clientsPing.count = count;
    }

    public AllPings(byte[] data)
    {
        clientsPing = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        byte[] data = new byte[DataSize];
        Buffer.BlockCopy(BitConverter.GetBytes(clientsPing.count), 0, data, 0, sizeof(int));
        Buffer.BlockCopy(clientsPing.ms, 0, data, sizeof(int), sizeof(short) * clientsPing.count);

        return GetFormattedData(data);
    }

    public override ClientsPing Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        ClientsPing incomingClientsPing;
        incomingClientsPing.count = BitConverter.ToInt32(payload, 0);
        incomingClientsPing.ms = new short[incomingClientsPing.count];

        for (int i = 0; i < incomingClientsPing.count; i++)
        {
            incomingClientsPing.ms[i] = BitConverter.ToInt16(payload, sizeof(int) + i * sizeof(short));
        }

        return incomingClientsPing;
    }
}