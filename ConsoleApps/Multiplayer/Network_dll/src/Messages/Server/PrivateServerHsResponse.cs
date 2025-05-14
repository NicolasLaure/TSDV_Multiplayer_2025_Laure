using System;
using Network.Enums;

namespace Network.Messages.Server
{
    public class PrivateServerHsResponse : Message<int>
    {
        public int id;

        PrivateServerHsResponse(int id)
        {
            messageType = MessageType.PrivateHsResponse;
            this.id = id;
            messageId++;
        }

        PrivateServerHsResponse(byte[] data)
        {
            id = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(id));
        }

        public override int Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            return BitConverter.ToInt32(payload);
        }
    }
}
