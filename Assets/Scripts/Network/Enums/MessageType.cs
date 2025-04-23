namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse,
        Acknowledge,
        DisAcknowledge, //ToDo
        Disconnect, //ToDo
        Ping,
        Position,
        Chat, //ToDo
        Console,
        Error,
    }
}