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
        Crouch = 17,
        Shoot = 18,
        InstantiateRequest = 19,
        DeInstantiateRequest = 20,
        InstantiateAll = 21,
        InstanceIntegrityCheck = 22,
        Username = 23,
        Usernames = 24,
        Win = 25,
        Death = 26,
        AxisInput = 27,
        ActionInput = 28,
        Primitive = 29,
        Rpc = 30,
        Error_UsernameTaken = 50,
        Error_AfkDisconnect = 51,
        Error_UserBanned = 52
    }
}