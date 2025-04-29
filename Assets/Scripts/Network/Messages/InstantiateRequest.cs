using System;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;

namespace Network.Messages
{
    public class InstantiateRequest : Message<InstanceData>
    {
        public InstanceData instanceData;

        public InstantiateRequest(InstanceData instanceData)
        {
            instanceData = instanceData;
        }

        public InstantiateRequest(byte[] data)
        {
            instanceData = Deserialize(data);
        }


        public override byte[] Serialize()
        {
            byte[] data = new byte[Marshal.SizeOf(instanceData)];
            int offset = 0;
            Buffer.BlockCopy(ByteFormat.Get4X4Bytes(instanceData.trs), 0, data, offset, sizeof(float) * 12);
            offset += sizeof(float) * 12;
            Buffer.BlockCopy(ByteFormat.GetVector4Bytes(instanceData.color), 0, data, offset, sizeof(float) * 4);

            return data;
        }

        public override InstanceData Deserialize(byte[] message)
        {
            InstanceData instanceData;

            int offset = 0;
            instanceData.trs = ByteFormat.Get4X4FromBytes(ExtractPayload(message), offset);
            offset += sizeof(float) * 12;
            instanceData.color = ByteFormat.GetVector4FromBytes(ExtractPayload(message), offset);
            return instanceData;
        }
    }
}