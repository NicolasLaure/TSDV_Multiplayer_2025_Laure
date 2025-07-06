using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Network.Enums;
using Utils;

namespace Network.Messages
{
    public class InstantiateRequest : Message<InstanceData>
    {
        public InstanceData instanceData;

        public InstantiateRequest(InstanceData instanceData)
        {
            messageType = MessageType.InstantiateRequest;
            attribs = Attributes.Important;
            this.instanceData = instanceData;
        }

        public InstantiateRequest(byte[] data)
        {
            messageType = MessageType.InstantiateRequest;
            attribs = Attributes.Important;
            instanceData = Deserialize(data);
            clientId = BitConverter.ToInt32(data, MessageOffsets.ClientIdIndex);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(instanceData.instanceID));
            data.AddRange(BitConverter.GetBytes(instanceData.originalClientID));
            data.AddRange(BitConverter.GetBytes(instanceData.prefabHash));
            data.AddRange(instanceData.trs);
            data.AddRange(BitConverter.GetBytes(instanceData.color));
            data.AddRange(BitConverter.GetBytes(instanceData.routeLength));
            for (int i = 0; i < instanceData.routeLength; i++)
                data.AddRange(BitConverter.GetBytes(instanceData.route[i]));

            return GetFormattedData(data.ToArray());
        }

        public override InstanceData Deserialize(byte[] message)
        {
            InstanceData instanceData;
            byte[] payload = ExtractPayload(message);
            int offset = 0;
            instanceData.instanceID = BitConverter.ToInt32(payload);
            offset += sizeof(int);
            instanceData.originalClientID = BitConverter.ToInt32(payload, offset);
            offset += sizeof(int);
            instanceData.prefabHash = BitConverter.ToUInt32(payload, offset);
            offset += sizeof(uint);
            instanceData.trs = payload[offset..(offset + Constants.MatrixSize)];
            offset += Constants.MatrixSize;
            instanceData.color = BitConverter.ToInt16(payload, offset);
            offset += sizeof(short);
            instanceData.routeLength = BitConverter.ToInt32(payload, offset);
            offset += sizeof(int);
            List<int> routeList = new List<int>();
            for (int i = 0; i < instanceData.routeLength; i++)
            {
                routeList.Add(BitConverter.ToInt32(payload, offset));
                offset += sizeof(int);
            }

            instanceData.route = routeList.ToArray();

            return instanceData;
        }
    }
}