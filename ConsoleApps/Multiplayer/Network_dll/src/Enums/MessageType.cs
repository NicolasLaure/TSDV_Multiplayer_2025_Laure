namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse = 0,
        MatchMakerHsResponse = 1,
        PrivateHandshake = 2,
        PrivateMatchMakerHandshake = 3,
        PrivateHsResponse = 4,
        PrivateMatchmakerHsResponse = 5,
        Acknowledge = 6,
        DisAcknowledge = 7, //ToDo
        Disconnect = 8,
        Ping = 9,
        AllPings = 10,
        ServerDirection = 11,
        Position = 12,
        Chat = 13, //ToDo
        ImportantOrderTest = 14,
        Console = 15,
        Error = 16,
    }
}