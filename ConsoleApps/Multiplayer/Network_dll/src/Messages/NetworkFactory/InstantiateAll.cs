using System;
using System.Collections.Generic;
using Utils;

namespace Network.Messages;

public class InstantiateAll
{
    public int count;
    public List<InstanceData> instancesData;

    public InstantiateAll(List<InstanceData> instancesData)
    {
        this.instancesData = instancesData;
        count = instancesData.Count;
    }

    public InstantiateAll(byte[] data)
    {
        instancesData = Deserialize(data);
        count = instancesData.Count;
    }


    public byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(count));
        for (int i = 0; i < instancesData.Count; i++)
        {
            data.AddRange(BitConverter.GetBytes(instancesData[i].instanceID));
            data.AddRange(BitConverter.GetBytes(instancesData[i].originalClientID));
            data.AddRange(BitConverter.GetBytes(instancesData[i].prefabHash));
            data.AddRange(instancesData[i].trs);
            data.AddRange(BitConverter.GetBytes(instancesData[i].color));
        }

        return data.ToArray();
    }

    public List<InstanceData> Deserialize(byte[] message)
    {
        List<InstanceData> instanceDatas = new List<InstanceData>();

        int count = BitConverter.ToInt32(message);
        for (int i = 0; i < count; i++)
        {
            InstanceData instanceData;
            int offset = 0;
            instanceData.instanceID = BitConverter.ToInt32(message);
            offset += sizeof(int);
            instanceData.originalClientID = BitConverter.ToInt32(message, offset);
            offset += sizeof(int);
            instanceData.prefabHash = BitConverter.ToUInt32(message, offset);
            offset += sizeof(uint);
            instanceData.trs = message[offset..(offset + Constants.MatrixSize)];
            offset += Constants.MatrixSize;
            instanceData.color = BitConverter.ToInt16(message, offset);

            instanceDatas.Add(instanceData);
        }

        return instanceDatas;
    }
}