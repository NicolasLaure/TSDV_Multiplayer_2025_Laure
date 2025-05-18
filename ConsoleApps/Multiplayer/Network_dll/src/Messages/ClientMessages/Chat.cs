using System;
using System.Collections.Generic;
using System.Text;
using Network;
using Network.Enums;

namespace Network_dll.Messages.ClientMessages;

public class Chat : Message<string>
{
    public string message;

    public Chat(string message)
    {
        messageType = MessageType.Chat;
        attribs = Attributes.Order | Attributes.Important;
        this.message = message;
        messageId++;
    }

    public Chat(byte[] data)
    {
        message = Deserialize(data);
    }

    public override byte[] Serialize()
    {
        List<byte> data = new List<byte>();
        for (int i = 0; i < message.Length; i++)
        {
            data.AddRange(BitConverter.GetBytes(message[i]));
        }

        return GetFormattedData(data.ToArray());
    }

    public override string Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        char[] text = Encoding.Unicode.GetChars(payload, 0, payload.Length);
        return new string(text);
    }
}