using System.Diagnostics;
using Utils;

namespace Network;

public struct InstanceData
{
    public int instanceID;
    public int originalClientID;
    public uint prefabHash;
    public byte[] trs;
    public short color;

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