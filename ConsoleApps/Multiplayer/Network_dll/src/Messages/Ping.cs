using System;
using Network.Enums;

namespace Network.Messages
{
    public class Ping : Message<double>
    {
        public double ms;

        public Ping(double ms)
        {
            messageType = MessageType.Ping;
            attribs = Attributes.None;
            this.ms = ms;
        }

        public Ping(byte[] data)
        {
            ms = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            return GetFormattedData(BitConverter.GetBytes(ms));
        }

        public override double Deserialize(byte[] message)
        {
            return BitConverter.ToInt16(ExtractPayload(message));
        }
    }
}