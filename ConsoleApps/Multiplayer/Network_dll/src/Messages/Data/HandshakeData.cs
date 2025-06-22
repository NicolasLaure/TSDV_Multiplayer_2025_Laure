namespace Network.Messages
{
    public struct HandshakeData
    {
        public int usernameLength;
        public string username;
        public bool isAuthServer;
    }
}