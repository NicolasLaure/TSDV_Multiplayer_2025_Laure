using System;
using System.Collections.Generic;
using Network.Enums;
using Utils;

namespace Network.Messages.Server
{
    public class ServerHsResponse : Message<ServerHandshakeResponseData>
    {
        public ServerHandshakeResponseData ServerHandshakeData;

        public ServerHsResponse(int id, int count, int seed, List<byte[]> players)
        {
            messageType = MessageType.HandShakeResponse;
            attribs = Attributes.Important;
            ServerHandshakeData.id = id;
            ServerHandshakeData.seed = seed;
            ServerHandshakeData.count = count;
            ServerHandshakeData.players = players;
            messageId++;
        }

        public ServerHsResponse(byte[] data)
        {
            ServerHandshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            int size = sizeof(int) * 3 + ServerHandshakeData.players.Count * (Constants.MatrixSize + sizeof(bool));
            byte[] data = new byte[size];

            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.id), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.seed), 0, data, 4, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.count), 0, data, 8, sizeof(int));

            int offset = sizeof(int) * 3;
            for (int i = 0; i < ServerHandshakeData.count; i++)
            {
                Buffer.BlockCopy(ServerHandshakeData.players[i], 0, data, offset, ServerHandshakeData.players[i].Length);
                offset += sizeof(bool) + Constants.MatrixSize;
            }

            return GetFormattedData(data);
        }

        public override ServerHandshakeResponseData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            ServerHandshakeResponseData data;
            data.id = BitConverter.ToInt32(payload, 0);
            data.seed = BitConverter.ToInt32(payload, sizeof(int));
            data.count = BitConverter.ToInt32(payload, sizeof(int) * 2);
            data.players = new List<byte[]>();

            int offset = sizeof(int) * 3;
            for (int i = 0; i < data.count; i++)
            {
                byte[] cube = payload[offset..(offset + sizeof(bool) + Constants.MatrixSize)];
                offset += sizeof(bool) + Constants.MatrixSize;
                data.players.Add(cube);
            }

            return data;
        }
    }
}