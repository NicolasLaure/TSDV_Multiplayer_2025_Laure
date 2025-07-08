using System;
using System.Collections.Generic;
using System.Numerics;

namespace Network.Messages
{
    public struct MouseInput
    {
        Vector2 position;

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(position.X));
            data.AddRange(BitConverter.GetBytes(position.Y));
            return data.ToArray();
        }

        public static MouseInput Deserialize(byte[] data)
        {
            MouseInput input;
            input.position.X = BitConverter.ToSingle(data);
            input.position.Y = BitConverter.ToSingle(data, sizeof(float));
            return input;
        }
    }

    public class MouseClickMessage : Message<MouseInput>
    {
        private MouseInput _input;

        public MouseClickMessage(MouseInput input, int clientId)
        {
            _input = input;
            this.clientId = clientId;
            messageId++;
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(_input.Serialize());
            return GetFormattedData(data.ToArray());
        }

        public override MouseInput Deserialize(byte[] message)
        {
            return MouseInput.Deserialize(ExtractPayload(message));
        }
    }
}