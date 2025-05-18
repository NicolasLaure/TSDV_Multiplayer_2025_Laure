using System;
using Network.Enums;

namespace Network.Messages
{
    public class PrivateMatchMakerHandshake : Message<int>
    {
        public int elo;

        public PrivateMatchMakerHandshake(int elo)
        {
            isEncrypted = true;
            messageType = MessageType.PrivateMatchMakerHandshake;
            attribs = Attributes.Important | Attributes.Checksum;
            messageId++;
            this.elo = elo;
        }

        public PrivateMatchMakerHandshake(byte[] data)
        {
            elo = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(elo));
        }

        public override int Deserialize(byte[] message)
        {
            return BitConverter.ToInt32(ExtractPayload(message));
        }
    }
}