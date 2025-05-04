using System;
using System.Collections.Generic;
using Cubes;
using Network.Enums;

namespace Network.Messages
{
    public class PublicHandshakeResponse : Message<HandshakeResponseData>
    {
        public HandshakeResponseData _handshakeData;

        public PublicHandshakeResponse(int id, int count, int seed, List<Cube> cubes)
        {
            messageType = MessageType.HandShakeResponse;
            attribs = Attributes.Important;
            _handshakeData.id = id;
            _handshakeData.seed = seed;
            _handshakeData.count = count;
            _handshakeData.cubes = cubes;
            this.messageId = 0;
        }

        public PublicHandshakeResponse(byte[] data)
        {
            _handshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            int size = sizeof(int) * 3 + _handshakeData.cubes.Count * (sizeof(float) * 3 + sizeof(bool));
            byte[] data = new byte[size];

            Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.id), 0, data, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.seed), 0, data, 4, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.count), 0, data, 8, sizeof(int));

            int offset = sizeof(int) * 3;
            for (int i = 0; i < _handshakeData.count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.cubes[i].isActive), 0, data, offset, sizeof(bool));
                offset += sizeof(bool);
                Buffer.BlockCopy(ByteFormat.GetVector3Bytes(_handshakeData.cubes[i].position), 0, data, offset, sizeof(float) * 3);
                offset += sizeof(float) * 3;
            }

            return GetFormattedData(data);
        }

        public override HandshakeResponseData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            HandshakeResponseData data;
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