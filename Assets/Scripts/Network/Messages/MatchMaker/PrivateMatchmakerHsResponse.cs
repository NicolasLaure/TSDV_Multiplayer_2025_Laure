using System;
using Network.Enums;

namespace Network.Messages.MatchMaker
{
    public class PrivateMatchmakerHsResponse : Message<int>
    {
        public int id;

        PrivateMatchmakerHsResponse(int id)
        {
            messageType = MessageType.Error;
            this.id = id;
        }

        PrivateMatchmakerHsResponse(byte[] data)
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