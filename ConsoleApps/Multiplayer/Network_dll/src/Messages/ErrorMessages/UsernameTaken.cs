using System;
using Network_dll.Messages.ErrorMessages;
using Network.Enums;

namespace Network.Messages.MatchMaker;

public class UsernameTaken : ErrorMessage
{
    public UsernameTaken()
    {
        messageType = MessageType.Error_UsernameTaken;
        attribs = Attributes.None;
    }
}