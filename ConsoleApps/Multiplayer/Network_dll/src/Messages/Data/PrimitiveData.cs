using Network.Enums;

namespace Network_dll.Messages.Data;

public struct PrimitiveData
{
    public PrimitiveType type;
    public int routeLength;
    public int[] route;
    public object obj;
}