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
        Position,
        Chat, //ToDo
        Console,
        Error,
    }
}