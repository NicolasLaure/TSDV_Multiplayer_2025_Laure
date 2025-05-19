using System;
using Network.Enums;

namespace Network.Messages
{
    public class ActionInput : Message<short>
    {
        public short actionType;

        public ActionInput(ActionType type)
        {
            messageType = MessageType.ActionInput;
            attribs = Attributes.Important;
            actionType = (short)type;
        }

        public ActionInput(byte[] data)
        {
            actionType = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(actionType));
        }

        public override short Deserialize(byte[] message)
        {
            return BitConverter.ToInt16(ExtractPayload(message));
        }
    }
}