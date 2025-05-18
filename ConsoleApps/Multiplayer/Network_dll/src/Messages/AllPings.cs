using System;
using System.Collections.Generic;
using Network;
using Network_dll.Messages.Data;
using Network.Enums;

public class AllPings : Message<ClientsPing>
{
    public ClientsPing clientsPing;

    private int DataSize => sizeof(int) + clientsPing.count * sizeof(short);

    public AllPings(ClientPing[] ms, int count)
    {
        messageType = MessageType.AllPings;
        attribs = Attributes.None;
        messageId++;
        clientsPing.clientPings = ms;
        clientsPing.count = count;
    }

    public AllPings(byte[] data)
    {
        clientsPing = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(clientsPing.count));
        for (int i = 0; i < clientsPing.count; i++)
        {
            data.AddRange(BitConverter.GetBytes(clientsPing.clientPings[i].id));
            data.AddRange(BitConverter.GetBytes(clientsPing.clientPings[i].ms));
        }

        return GetFormattedData(data.ToArray());
    }

    public override ClientsPing Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        ClientsPing incomingClientsPing;
        incomingClientsPing.count = BitConverter.ToInt32(payload, 0);
        List<ClientPing> pings = new List<ClientPing>();
        int offset = sizeof(int);
        for (int i = 0; i < incomingClientsPing.count; i++)
        {
            ClientPing ping;
            ping.id = BitConverter.ToInt32(payload, offset);
            offset += sizeof(int);
            ping.ms = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);
            pings.Add(ping);
        }

        incomingClientsPing.clientPings = pings.ToArray();

        return incomingClientsPing;
    }
}