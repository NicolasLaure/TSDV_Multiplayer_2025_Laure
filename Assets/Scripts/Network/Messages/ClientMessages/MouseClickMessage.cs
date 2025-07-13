using System;
using System.Collections.Generic;
using System.Numerics;
using Network.Enums;

namespace Network.Messages
{
    public struct MouseInput
    {
        public float x;
        public float y;

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(x));
            data.AddRange(BitConverter.GetBytes(y));
            return data.ToArray();
        }

        public static MouseInput Deserialize(byte[] data)
        {
            MouseInput input;
            input.x = BitConverter.ToSingle(data);
            input.y = BitConverter.ToSingle(data, sizeof(float));
            return input;
        }
    }

    public class MouseClickMessage : Message<MouseInput>
    {
        public MouseInput input;

        public MouseClickMessage(MouseInput input)
        {
            messageType = MessageType.MouseInput;
            this.input = input;
            clientId = -1;
            messageId++;
        }

        public MouseClickMessage(byte[] data)
        {
            messageType = MessageType.MouseInput;
            input = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(input.Serialize());
            return GetFormattedData(data.ToArray());
        }

        public override MouseInput Deserialize(byte[] message)
        {
            return MouseInput.Deserialize(ExtractPayload(message));
        }
    }
}