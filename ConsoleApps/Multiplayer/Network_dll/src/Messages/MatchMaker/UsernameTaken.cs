using System;
using Network.Enums;

namespace Network.Messages.MatchMaker;

public class UsernameTaken : Message<short>
{
    public UsernameTaken()
    {
        messageType = MessageType.UsernameTaken;
        attribs = Attributes.None;
    }

    public override byte[] Serialize()
    {
        return GetFormattedData(Array.Empty<byte>());
    }

    public override short Deserialize(byte[] message)
    {
        return 0;
    }
}