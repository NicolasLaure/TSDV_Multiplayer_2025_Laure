using System;
using System.Collections.Generic;
using Network.Enums;
using Utils;

namespace Network.Messages;

public class InstanceIntegrityCheck : Message<InstanceData>
{
    public InstanceData instanceData;

    public InstanceIntegrityCheck(InstanceData instanceData)
    {
        messageType = MessageType.InstanceIntegrityCheck;
        attribs = Attributes.Important;
        this.instanceData = instanceData;
    }

    public InstanceIntegrityCheck(byte[] data)
    {
        messageType = MessageType.InstanceIntegrityCheck;
        attribs = Attributes.Important;
        instanceData = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(instanceData.instanceID));
        data.AddRange(BitConverter.GetBytes(instanceData.originalClientID));
        data.AddRange(BitConverter.GetBytes(instanceData.prefabHash));
        data.AddRange(instanceData.trs);
        data.AddRange(BitConverter.GetBytes(instanceData.color));

        return GetFormattedData(data.ToArray());
    }

    public override InstanceData Deserialize(byte[] message)
    {
        InstanceData receivedInstanceData;
        byte[] payload = ExtractPayload(message);
        int offset = 0;
        receivedInstanceData.instanceID = BitConverter.ToInt32(payload);
        offset += sizeof(int);
        receivedInstanceData.originalClientID = BitConverter.ToInt32(payload, offset);
        offset += sizeof(int);
        receivedInstanceData.prefabHash = BitConverter.ToUInt32(payload, offset);
        offset += sizeof(uint);
        receivedInstanceData.trs = payload[offset..(offset + Constants.MatrixSize)];
        offset += Constants.MatrixSize;
        receivedInstanceData.color = BitConverter.ToInt16(payload, offset);
        return receivedInstanceData;
    }
}