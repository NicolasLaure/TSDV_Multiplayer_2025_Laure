namespace Network.Enums
{
    public enum MessageType : short
    {
        HandShake = -1,
        HandShakeResponse,
        Acknowledge,
        DisAcknowledge, //To Do
        Disconnect, //To Do
        Ping,
        Position,
        Chat, //To Do
        Console,
        Error,
    }
}