using System;
using System.Collections.Generic;
using System.Text;
using Network.Enums;

namespace Network.Messages.Server;

public class UsernameMessage : Message<OtherUsername>
{
    public OtherUsername username;

    public UsernameMessage(OtherUsername username)
    {
        messageType = MessageType.Username;
        attribs = Attributes.Checksum;
        this.username = this.username;
    }
    public UsernameMessage(byte[] data)
    {
        username = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(username.id));
        data.AddRange(BitConverter.GetBytes(username.usernameLength));
        for (int i = 0; i < username.usernameLength; i++)
        {
            data.AddRange(BitConverter.GetBytes(username.username[i]));
        }

        return data.ToArray();
    }

    public override OtherUsername Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        OtherUsername otherUsername;
        otherUsername.id = BitConverter.ToInt32(payload);
        int offset = sizeof(int);
        otherUsername.usernameLength = BitConverter.ToInt32(payload, offset);
        offset += sizeof(int);
        char[] usernameChars = Encoding.Unicode.GetChars(payload, offset, otherUsername.usernameLength * 2);
        otherUsername.username = new string(usernameChars);
        return otherUsername;
    }
}