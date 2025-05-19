using System;
using Network;

namespace Network_dll.Messages.ErrorMessages;

public abstract class ErrorMessage : Message<char>
{
    public override byte[] Serialize()
    {
        return GetFormattedData(Array.Empty<byte>());
    }

    public override char Deserialize(byte[] message)
    {
        return 'E';
    }
}