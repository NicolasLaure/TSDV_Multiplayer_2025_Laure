using System;
using Network.Enums;
using UnityEngine;
using Utils;

namespace Network.Messages
{
    public class Position : Message<(Matrix4x4, int)>
    {
        public Matrix4x4 trs;
        public int instanceID;

        public Position(Matrix4x4 pos, int instanceID, int clientId)
        {
            messageType = MessageType.Position;
            attribs = Attributes.Order;
            this.trs = pos;
            this.instanceID = instanceID;
            this.clientId = clientId;
            messageId++;
        }

        public Position(byte[] data)
        {
            (Matrix4x4, int) posAndIndex = Deserialize(data);
            trs = posAndIndex.Item1;
            instanceID = posAndIndex.Item2;
            clientId = GetClientId(data);
        }

        public override byte[] Serialize()
        {
            byte[] data = new byte[sizeof(int) + Constants.MatrixSize];
            Buffer.BlockCopy(BitConverter.GetBytes(instanceID), 0, data, 0, 4);
            Buffer.BlockCopy(ByteFormat.Get4X4Bytes(trs), 0, data, 4, Constants.MatrixSize);

            return GetFormattedData(data);
        }

        public override (Matrix4x4, int) Deserialize(byte[] message)
        {
            byte[] data = ExtractPayload(message);
            int index = BitConverter.ToInt32(data, 0);
            byte[] componentsData = new byte[Constants.MatrixSize];
            Buffer.BlockCopy(data, 4, componentsData, 0, Constants.MatrixSize);
            return (ByteFormat.Get4X4FromBytes(componentsData, 0), index);
        }
    }
}