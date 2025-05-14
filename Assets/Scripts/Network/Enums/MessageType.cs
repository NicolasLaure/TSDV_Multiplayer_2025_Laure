namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse,
        MatchMakerHsResponse,
        PrivateHandshake,
        PrivateMatchMakerHandshake,
        PrivateHsResponse,
        PrivateMatchmakerHsResponse,
        Acknowledge,
        DisAcknowledge, //ToDo
        Disconnect,
        Ping,
        AllPings,
        ServerDirection,
        Position,
        Chat, //ToDo
        ImportantOrderTest,
        Console,
        Error,
    }
}