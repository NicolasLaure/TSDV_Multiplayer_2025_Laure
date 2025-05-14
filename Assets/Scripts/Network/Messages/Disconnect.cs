using System;
using Network.Enums;

namespace Network.Messages
{
    public class Disconnect : Message<Disconnect>
    {
        public int id;

        public Disconnect(int id)
        {
            messageType = MessageType.Disconnect;
            attribs = Attributes.None;
            messageId++;
            this.id = id;
        }

        public Disconnect(byte[] data)
        {
            id = Deserialize(data).id;
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(id));
        }

        public override Disconnect Deserialize(byte[] message)
        {
            return new Disconnect(BitConverter.ToInt32(ExtractPayload(message), 0));
        }
    }
}