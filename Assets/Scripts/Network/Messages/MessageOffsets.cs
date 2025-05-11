namespace Network.Messages
{
    public class MessageOffsets
    {
        internal static int IsEncryptedIndex => (0);
        internal static int MessageTypeIndex => (sizeof(bool));
        internal static int AttribsIndex => (sizeof(bool) + sizeof(short));
        internal static int IdIndex => (sizeof(bool) + sizeof(short) * 2);
        internal static int StartIndex => (sizeof(bool) + sizeof(int) + sizeof(short) * 2);
        internal static int EndIndex => (sizeof(bool) + sizeof(int) + sizeof(short) * 3);
    }
}