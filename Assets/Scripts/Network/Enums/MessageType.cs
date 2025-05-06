namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse,
        PrivateHandshake,
        Acknowledge,
        DisAcknowledge, //ToDo
        Disconnect,
        Ping,
        AllPings,
        Position,
        Chat, //ToDo
        Console,
        Error,
    }
}