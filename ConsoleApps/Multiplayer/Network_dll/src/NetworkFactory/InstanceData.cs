using System;
using Utils;

namespace Network;

public struct InstanceData
{
    public int instanceID;
    public int originalClientID;
    public uint prefabHash;
    public byte[] trs;
    public short color;
    public int routeLength;
    public int[] route;

    public InstanceData()
    {
        instanceID = 0;
        originalClientID = 0;
        prefabHash = 0;
        trs = new byte[Constants.MatrixSize];
        color = 0;
        routeLength = 0;
        route = Array.Empty<int>();
    }

    public static int SizeOf => (sizeof(int) * 3 + Constants.MatrixSize + sizeof(short));

    public static bool operator ==(InstanceData left, InstanceData right)
    {
        return left.instanceID == right.instanceID && left.originalClientID == right.originalClientID &&
               left.prefabHash == right.prefabHash && left.color == right.color;
    }

    public static bool operator !=(InstanceData left, InstanceData right)
    {
        return !(left == right);
    }
}