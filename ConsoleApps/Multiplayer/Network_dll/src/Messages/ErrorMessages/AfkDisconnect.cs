using Network_dll.Messages.ErrorMessages;
using Network.Enums;

namespace Network.Messages;

public class AfkDisconnect : ErrorMessage
{
   public AfkDisconnect()
   {
      messageType = MessageType.Error_AfkDisconnect;
      attribs = Attributes.None;
   }
}