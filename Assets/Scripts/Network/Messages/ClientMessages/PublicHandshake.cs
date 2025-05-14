using System;
using System.Runtime.InteropServices;
using Network.Enums;
using UnityEngine;

namespace Network.Messages
{
    public class PublicHandshake : Message<HandshakeData>
    {
        private HandshakeData _handshakeData;

        public PublicHandshake(HandshakeData handshakeData)
        {
            messageType = MessageType.HandShake;
            attribs = Attributes.Important | Attributes.Critical;
            messageId++;
            _handshakeData = handshakeData;
        }

        public PublicHandshake(byte[] data)
        {
            _handshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            int size = Marshal.SizeOf(_handshakeData);
            byte[] data = new byte[size];

            Buffer.BlockCopy(BitConverter.GetBytes(_handshakeData.ip), 0, data, 0, sizeof(int));
            return GetFormattedData(data);
        }

        public override HandshakeData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            HandshakeData data;
            data.ip = BitConverter.ToInt32(payload, 0);
            return data;
        }
    }
}