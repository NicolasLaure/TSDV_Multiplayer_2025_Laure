using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Network;
using Network.Enums;

namespace Network_dll.Messages.ClientMessages
{
    public class Win : Message<(string winner, string loser)>
    {
        public int winnerNameSize;
        public string winnerUsername;
        public int loserNameSize;
        public string loserUsername;

        public Win(string winnerUsername, string loserUsername)
        {
            messageType = MessageType.Win;
            attribs = Attributes.None;
            this.winnerUsername = winnerUsername;
            winnerNameSize = winnerUsername.Length;
            this.loserUsername = loserUsername;
            loserNameSize = this.loserUsername.Length;
        }

        public Win(byte[] data)
        {
            (winnerUsername, loserUsername) = Deserialize(data);
            winnerNameSize = winnerUsername.Length;
            loserNameSize = loserUsername.Length;
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(winnerNameSize));
            for (int i = 0; i < winnerNameSize; i++)
            {
                data.AddRange(BitConverter.GetBytes(winnerUsername[i]));
            }

            data.AddRange(BitConverter.GetBytes(loserNameSize));
            for (int i = 0; i < loserNameSize; i++)
            {
                data.AddRange(BitConverter.GetBytes(loserUsername[i]));
            }

            return GetFormattedData(data.ToArray());
        }

        public override (string winner, string loser) Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            int winnerSize = BitConverter.ToInt32(payload);
            int offset = sizeof(int);
            char[] winnerName = Encoding.Unicode.GetChars(payload, offset, winnerSize * 2);
            offset += winnerSize * 2;
            int loserSize = BitConverter.ToInt32(payload, offset);
            offset += sizeof(int);
            char[] loserName = Encoding.Unicode.GetChars(payload, offset, loserSize * 2);

            return (new string(winnerName), new string(loserName));
        }
    }
}