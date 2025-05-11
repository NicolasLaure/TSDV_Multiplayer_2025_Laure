using System;
using Network.Enums;

namespace Network.Messages
{
    public class PrivateHandshake : Message<int>
    {
        public int id;

        public PrivateHandshake(int id, int messageId)
        {
            isEncrypted = true;
            messageType = MessageType.PrivateHandshake;
            attribs = Attributes.Important | Attributes.Checksum;
            this.messageId = messageId;
            this.id = id;
        }

        public PrivateHandshake(byte[] data)
        {
            id = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(id));
        }

        public override int Deserialize(byte[] message)
        {
            return BitConverter.ToInt32(ExtractPayload(message));
        }
    }
}