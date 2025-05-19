using System;
using Network.Enums;

namespace Network.Messages.MatchMaker
{
    public class PrivateMatchmakerHsResponse : Message<int>
    {
        public int elo;

        public PrivateMatchmakerHsResponse(int elo)
        {
            isEncrypted = true;
            messageType = MessageType.PrivateMatchmakerHsResponse;
            attribs = Attributes.Important | Attributes.Checksum;
            this.elo = elo;
            messageId++;
        }

        public PrivateMatchmakerHsResponse(byte[] data)
        {
            elo = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(elo));
        }

        public override int Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            return BitConverter.ToInt32(payload);
        }
    }
}