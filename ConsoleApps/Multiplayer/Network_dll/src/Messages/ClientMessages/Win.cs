using System;
using System.Collections.Generic;
using System.Text;
using Network;
using Network.Enums;

namespace Network_dll.Messages.ClientMessages
{
    public class Win : Message<string>
    {
        public int nameSize;
        public string winnerUsername;

        public Win(string winnerUsername)
        {
            messageType = MessageType.Win;
            attribs = Attributes.None;
            this.winnerUsername = winnerUsername;
            nameSize = winnerUsername.Length;
        }

        public Win(byte[] data)
        {
            winnerUsername = Deserialize(data);
            nameSize = winnerUsername.Length;
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(nameSize));
            for (int i = 0; i < nameSize; i++)
            {
                data.AddRange(BitConverter.GetBytes(winnerUsername[i]));
            }

            return GetFormattedData(data.ToArray());
        }

        public override string Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            int size = BitConverter.ToInt32(payload);
            char[] name = Encoding.Unicode.GetChars(payload, sizeof(int), size * 2);
            return new string(name);
        }
    }
}