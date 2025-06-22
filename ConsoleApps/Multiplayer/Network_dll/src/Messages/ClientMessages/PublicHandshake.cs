using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Network.Enums;

namespace Network.Messages
{
    public class PublicHandshake : Message<HandshakeData>
    {
        public HandshakeData handshakeData;

        public PublicHandshake(HandshakeData handshakeData)
        {
            messageType = MessageType.HandShake;
            attribs = Attributes.Important | Attributes.Critical;
            messageId++;
            this.handshakeData = handshakeData;
        }

        public PublicHandshake(byte[] data)
        {
            handshakeData = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(handshakeData.usernameLength));
            for (int i = 0; i < handshakeData.usernameLength; i++)
            {
                data.AddRange(BitConverter.GetBytes(handshakeData.username[i]));
            }

            data.AddRange(BitConverter.GetBytes(handshakeData.isAuthServer));

            return GetFormattedData(data.ToArray());
        }

        public override HandshakeData Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            HandshakeData data;
            data.usernameLength = BitConverter.ToInt32(payload);
            int offset = sizeof(int);
            char[] username = Encoding.Unicode.GetChars(payload, offset, data.usernameLength * 2);
            data.username = new string(username);
            offset += data.usernameLength * 2;
            data.isAuthServer = BitConverter.ToBoolean(payload, offset);
            return data;
        }
    }
}