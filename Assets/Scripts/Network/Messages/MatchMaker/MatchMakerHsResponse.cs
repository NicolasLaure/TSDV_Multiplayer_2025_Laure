using System;
using Network.Enums;
using Network.Messages.Data;

namespace Network.Messages.MatchMaker
{
    public class MatchMakerHsResponse : Message<MatchMakerHsResponseData>
    {
        public MatchMakerHsResponseData MatchMakerHandshakeData;

        public MatchMakerHsResponse(int id, int seed)
        {
            messageType = MessageType.MatchMakerHsResponse;
            attribs = Attributes.Important;
            MatchMakerHandshakeData.id = id;
            MatchMakerHandshakeData.seed = seed;
            messageId++;
        }

        public MatchMakerHsResponse(byte[] data)
        {
            MatchMakerHandshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            int size = sizeof(int) * 2;
            byte[] data = new byte[size];

            Buffer.BlockCopy(BitConverter.GetBytes(MatchMakerHandshakeData.id), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(MatchMakerHandshakeData.seed), 0, data, 4, sizeof(int));

            return GetFormattedData(data);
        }

        public override MatchMakerHsResponseData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            MatchMakerHsResponseData data;
            data.id = BitConverter.ToInt32(payload, 0);
            data.seed = BitConverter.ToInt32(payload, sizeof(int));
            return data;
        }
    }
}