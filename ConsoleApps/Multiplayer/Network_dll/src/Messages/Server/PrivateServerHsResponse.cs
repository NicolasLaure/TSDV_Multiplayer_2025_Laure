using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Network.Enums;
using Network.Utilities;

namespace Network.Messages.Server
{
    public class PrivateServerHsResponse : Message<InstantiateAll>
    {
        public InstantiateAll objectsToInstantiate;

        public PrivateServerHsResponse(InstantiateAll objectsToInstantiate)
        {
            isEncrypted = true;
            messageType = MessageType.PrivateHsResponse;
            attribs = Attributes.Important | Attributes.Checksum;
            this.objectsToInstantiate = objectsToInstantiate;
            messageId++;
        }

        public PrivateServerHsResponse(byte[] data)
        {
            objectsToInstantiate = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(objectsToInstantiate.Serialize());
            return GetFormattedData(data.ToArray());
        }

        public override InstantiateAll Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            return new InstantiateAll(payload);
        }
    }
}