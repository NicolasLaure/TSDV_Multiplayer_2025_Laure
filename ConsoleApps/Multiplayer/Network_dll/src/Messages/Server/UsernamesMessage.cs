using System;
using System.Collections.Generic;
using System.Text;
using Network.Enums;
using Network.Utilities;

namespace Network.Messages.Server
{
    public class UsernamesMessage : Message<OtherUsername[]>
    {
        public int usernamesCount;
        public OtherUsername[] usernames;

        public UsernamesMessage(OtherUsername[] otherUsernames)
        {
            messageType = MessageType.Usernames;
            attribs = Attributes.Checksum;
            usernamesCount = otherUsernames.Length;
            this.usernames = otherUsernames;
        }

        public UsernamesMessage(byte[] data)
        {
            messageType = MessageType.Usernames;
            attribs = Attributes.Checksum;
            usernames = Deserialize(data);
            usernamesCount = usernames.Length;
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(usernamesCount));
            Logger.Log($"UsernamesCount: {usernamesCount}");
            for (int i = 0; i < usernamesCount; i++)
            {
                data.AddRange(BitConverter.GetBytes(usernames[i].id));
                data.AddRange(BitConverter.GetBytes(usernames[i].usernameLength));
                for (int j = 0; j < usernames[i].usernameLength; j++)
                {
                    data.AddRange(BitConverter.GetBytes(usernames[i].username[j]));
                }
            }

            return GetFormattedData(data.ToArray());
        }

        public override OtherUsername[] Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            int count = BitConverter.ToInt32(payload);
            int offset = sizeof(int);
            List<OtherUsername> otherUsernames = new List<OtherUsername>();
            for (int i = 0; i < count; i++)
            {
                OtherUsername otherUsername;
                otherUsername.id = BitConverter.ToInt32(payload, offset);
                offset += sizeof(int);
                otherUsername.usernameLength = BitConverter.ToInt32(payload, offset);
                offset += sizeof(int);
                Logger.Log($"Length:{payload.Length} Offset: {offset}, Offset+UsernameLength{offset + otherUsername.usernameLength * 2}");
                char[] usernameChars = Encoding.Unicode.GetChars(payload, offset, otherUsername.usernameLength * 2);
                offset += otherUsername.usernameLength * 2;
                otherUsername.username = new string(usernameChars);

                otherUsernames.Add(otherUsername);
            }

            return otherUsernames.ToArray();
        }
    }
}