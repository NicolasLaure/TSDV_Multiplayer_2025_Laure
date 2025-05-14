using System;
using Network.Enums;

namespace Network.Messages.TestMessages
{
    public class ImportantOrderMessage : Message<int>
    {
        public int number;

        public ImportantOrderMessage(int id)
        {
            messageType = MessageType.ImportantOrderTest;
            attribs = Attributes.Important | Attributes.Order;
            messageId = id;
            number = id + 1;
        }

        public ImportantOrderMessage(byte[] data)
        {
            messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            attribs = (Attributes)BitConverter.ToInt16(data, MessageOffsets.AttribsIndex);
            messageId = BitConverter.ToInt32(data, MessageOffsets.IdIndex);
            number = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(number));
        }

        public override int Deserialize(byte[] message)
        {
            return BitConverter.ToInt32(ExtractPayload(message));
        }
    }
}