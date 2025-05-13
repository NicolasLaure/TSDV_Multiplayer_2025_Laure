namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse,
        MatchMakerHandshakeResponse,
        PrivateMatchMakerHandshake,
        PrivateHandshake,
        Acknowledge,
        DisAcknowledge, //ToDo
        Disconnect,
        Ping,
        AllPings,
        Position,
        Chat, //ToDo
        ImportantOrderTest,
        Console,
        Error,
    }
}