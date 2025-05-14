namespace Network.SaveStructures
{
    public class CriticalMessage
    {
        public int clientId;
        public int messageId;
        public byte[] message;

        public CriticalMessage(int clientId, int messageId, byte[] message)
        {
            this.clientId = clientId;
            this.messageId = messageId;
        }
    }
}