namespace Network.Messages
{
    public class MessageOffsets
    {
        internal static int IsEncryptedIndex => (0);
        internal static int MessageTypeIndex => (sizeof(bool));
        internal static int AttribsIndex => (sizeof(bool) + sizeof(short));
        internal static int ClientIdIndex => (sizeof(bool) + sizeof(short) * 2);
        internal static int IdIndex => (sizeof(bool) + sizeof(short) * 2 + sizeof(int));
        internal static int StartIndex => (sizeof(bool) + sizeof(int) * 2 + sizeof(short) * 2);
        internal static int EndIndex => (sizeof(bool) + sizeof(int) * 2 + sizeof(short) * 3);
    }
}