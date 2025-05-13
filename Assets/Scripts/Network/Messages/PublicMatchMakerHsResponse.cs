using System;
using Network;
using Network.Enums;
using Network.Messages.Data;

public class PublicMatchMakerHsResponse : Message<MatchMakerHsResponseData>
{
    public MatchMakerHsResponseData MatchMakerHandshakeData;

    public PublicMatchMakerHsResponse(int id, int seed)
    {
        messageType = MessageType.MatchMakerHandshakeResponse;
        attribs = Attributes.Important;
        MatchMakerHandshakeData.id = id;
        MatchMakerHandshakeData.seed = seed;
        this.messageId = 0;
    }

    public PublicMatchMakerHsResponse(byte[] data)
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