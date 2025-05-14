using Network.Utilities;

namespace Network
{
    public class HeldMessage
    {
        public int id;
        public byte[] message;
        public float heldSince;

        public HeldMessage(int id, byte[] message)
        {
            this.id = id;
            this.message = message;
            heldSince = Time.time;
        }
    }
}