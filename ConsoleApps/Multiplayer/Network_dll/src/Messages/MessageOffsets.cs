namespace Network.Messages
{
    public class MessageOffsets
    {
        public static int IsEncryptedIndex => (0);
        public static int MessageTypeIndex => (sizeof(bool));
        public static int AttribsIndex => (sizeof(bool) + sizeof(short));
        public static int ClientIdIndex => (sizeof(bool) + sizeof(short) * 2);
        public static int IdIndex => (sizeof(bool) + sizeof(short) * 2 + sizeof(int));
        public static int StartIndex => (sizeof(bool) + sizeof(int) * 2 + sizeof(short) * 2);
        public static int EndIndex => (sizeof(bool) + sizeof(int) * 2 + sizeof(short) * 3);
    }
}