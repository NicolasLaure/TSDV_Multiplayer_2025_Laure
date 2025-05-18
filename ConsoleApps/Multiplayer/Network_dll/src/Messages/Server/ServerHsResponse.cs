using System;
using System.Collections.Generic;
using Network.Enums;
using Utils;

namespace Network.Messages.Server
{
    public class ServerHsResponse : Message<ServerHandshakeResponseData>
    {
        public ServerHandshakeResponseData ServerHandshakeData;

        public ServerHsResponse(int id, int seed)
        {
            messageType = MessageType.HandShakeResponse;
            attribs = Attributes.Important;
            ServerHandshakeData.id = id;
            ServerHandshakeData.seed = seed;
            messageId++;
        }

        public ServerHsResponse(byte[] data)
        {
            ServerHandshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(ServerHandshakeData.id));
            data.AddRange(BitConverter.GetBytes(ServerHandshakeData.seed));

            return GetFormattedData(data.ToArray());
        }

        public override ServerHandshakeResponseData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            ServerHandshakeResponseData data;
            data.id = BitConverter.ToInt32(payload, 0);
            data.seed = BitConverter.ToInt32(payload, sizeof(int));
            return data;
        }
    }
}