using Network.Enums;

namespace Network_dll.Messages.ErrorMessages;

public class UserIsBanned : ErrorMessage
{
    public UserIsBanned()
    {
        messageType = MessageType.Error_UserBanned;
        attribs = Attributes.None;
    }
}