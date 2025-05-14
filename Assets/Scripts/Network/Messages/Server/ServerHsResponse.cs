using System;
using System.Collections.Generic;
using Cubes;
using Network.Enums;

namespace Network.Messages.Server
{
    public class ServerHsResponse : Message<ServerHandshakeResponseData>
    {
        public ServerHandshakeResponseData ServerHandshakeData;

        public ServerHsResponse(int id, int count, int seed, List<Cube> cubes)
        {
            messageType = MessageType.HandShakeResponse;
            attribs = Attributes.Important;
            ServerHandshakeData.id = id;
            ServerHandshakeData.seed = seed;
            ServerHandshakeData.count = count;
            ServerHandshakeData.cubes = cubes;
            messageId++;
        }

        public ServerHsResponse(byte[] data)
        {
            ServerHandshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            int size = sizeof(int) * 3 + ServerHandshakeData.cubes.Count * (sizeof(float) * 3 + sizeof(bool));
            byte[] data = new byte[size];

            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.id), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.seed), 0, data, 4, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.count), 0, data, 8, sizeof(int));

            int offset = sizeof(int) * 3;
            for (int i = 0; i < ServerHandshakeData.count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(ServerHandshakeData.cubes[i].isActive), 0, data, offset, sizeof(bool));
                offset += sizeof(bool);
                Buffer.BlockCopy(ByteFormat.GetVector3Bytes(ServerHandshakeData.cubes[i].position), 0, data, offset, sizeof(float) * 3);
                offset += sizeof(float) * 3;
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
            data.cubes = new List<Cube>();

            int offset = sizeof(int) * 3;
            for (int i = 0; i < data.count; i++)
            {
                Cube cube = new Cube();
                cube.isActive = BitConverter.ToBoolean(payload, offset);
                offset += sizeof(bool);
                cube.position = ByteFormat.GetVector3FromBytes(payload, offset);
                offset += sizeof(float) * 3;
                data.cubes.Add(cube);
            }

            return data;
        }
    }
}