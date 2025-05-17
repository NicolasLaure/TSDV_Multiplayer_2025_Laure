using Network.Enums;
using Utils;

namespace Network.Messages
{
    public class Position : Message<(byte[], int)>
    {
        public byte[] trs;
        public int instanceID;

        public Position(byte[] trs, int instanceID)
        {
            messageType = MessageType.Position;
            attribs = Attributes.Order;
            this.trs = trs;
            this.instanceID = instanceID;
            messageId++;
        }

        public Position(byte[] data)
        {
            (byte[], int) posAndIndex = Deserialize(data);
            trs = posAndIndex.Item1;
            instanceID = posAndIndex.Item2;
        }

        public override byte[] Serialize()
        {
            byte[] data = new byte[sizeof(int) + Constants.MatrixSize];
            Buffer.BlockCopy(BitConverter.GetBytes(instanceID), 0, data, 0, 4);
            Buffer.BlockCopy(trs, 0, data, 4, Constants.MatrixSize);

            return GetFormattedData(data);
        }

        public override (byte[], int) Deserialize(byte[] message)
        {
            byte[] data = ExtractPayload(message);
            int index = BitConverter.ToInt32(data, 0);
            byte[] componentsData = new byte[Constants.MatrixSize];
            Buffer.BlockCopy(data, 4, componentsData, 0, Constants.MatrixSize);
            return (componentsData, index);
        }
    }
}