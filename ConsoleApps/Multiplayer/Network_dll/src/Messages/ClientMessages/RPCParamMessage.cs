using System;
using System.Collections.Generic;
using Network;
using Network_dll.Messages.Data;
using Network.Enums;
using Network.Messages;

namespace Network_dll.Messages.ClientMessages;

public class RPCParamMessage : Message<>
{
    public RpcData data;
    public object[] args;
    
    public RPCParamMessage(RpcData data, Attributes attributes, object[] args)
    {
        messageType = MessageType.Rpc;
        attribs = attributes;
        messageId++;
        this.data = data;
    }

    public RPCParamMessage(byte[] message)
    {
        messageType = MessageType.Rpc;
        attribs = Attributes.None;
        this.data = Deserialize(message);

        clientId = BitConverter.ToInt32(message, MessageOffsets.ClientIdIndex);
        messageId = BitConverter.ToInt32(message, MessageOffsets.IdIndex);
    }

    public override byte[] Serialize()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(data.routeLength));
        for (int i = 0; i < data.routeLength; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(data.route[i]));
        }

        return GetFormattedData(bytes.ToArray());
    }

    public override RpcData Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        RpcData newData;
        newData.routeLength = BitConverter.ToInt32(payload);
        int offset = sizeof(int);
        List<int> route = new List<int>();
        for (int i = 0; i < newData.routeLength; i++)
        {
            route.Add(BitConverter.ToInt32(payload, offset));
            offset += sizeof(int);
        }

        newData.route = route.ToArray();
        return newData;
    }
}


