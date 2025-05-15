using System;
using System.Numerics;
using Network.Enums;

namespace Network.Messages
{
    public class Position : Message<(byte[], int)>
    {
        public byte[] pos;
        public int instanceID;

        public Position(byte[] pos, int instanceID)
        {
            messageType = MessageType.Position;
            attribs = Attributes.Order;
            this.pos = pos;
            this.instanceID = instanceID;
            messageId++;
        }

        public Position(byte[] data)
        {
            (byte[], int) posAndIndex = Deserialize(data);
            pos = posAndIndex.Item1;
            instanceID = posAndIndex.Item2;
        }

        public override byte[] Serialize()
        {
            byte[] data = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(instanceID), 0, data, 0, 4);
            Buffer.BlockCopy(pos, 0, data, 4, 12);

            return GetFormattedData(data);
        }

        public override (byte[], int) Deserialize(byte[] message)
        {
            byte[] data = ExtractPayload(message);
            int index = BitConverter.ToInt32(data, 0);
            byte[] componentsData = new byte[12];
            Buffer.BlockCopy(data, 4, componentsData, 0, 12);
            return (componentsData, index);
        }
    }
}