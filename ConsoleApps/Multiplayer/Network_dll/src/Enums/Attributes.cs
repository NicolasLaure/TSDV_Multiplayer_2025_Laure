using System;

namespace Network.Enums
{
    [Flags]
    public enum Attributes : short
    {
        None = 1 << 0,
        Important = 1 << 2,
        Checksum = 1 << 3,
        Critical = 1 << 4,
        Order = 1 << 5
    }
}