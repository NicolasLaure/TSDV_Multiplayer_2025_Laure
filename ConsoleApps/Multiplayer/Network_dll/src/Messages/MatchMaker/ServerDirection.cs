using System;
using System.Net;
using System.Threading.Tasks;
using Network.Enums;

namespace Network.Messages.MatchMaker
{
    public class ServerDirection : Message<(IPAddress ip, int port)>
    {
        public IPAddress serverIp;
        public int serverPort;

        public ServerDirection(IPAddress ip, int port)
        {
            messageType = MessageType.ServerDirection;
            attribs = Attributes.Important | Attributes.Checksum;
            messageId++;
            serverIp = ip;
            serverPort = port;
        }

        public ServerDirection(byte[] data)
        {
            (serverIp, serverPort) = Deserialize(data);
        }

        public override byte[] Serialize()
        {
            byte[] address = serverIp.GetAddressBytes();
            byte[] dataToSend = new byte[address.Length + sizeof(int)];
            Buffer.BlockCopy(address, 0, dataToSend, 0, address.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(serverPort), 0, dataToSend, address.Length, sizeof(int));

            return GetFormattedData(dataToSend);
        }

        public override (IPAddress ip, int port) Deserialize(byte[] message)
        {
            byte[] payload = ExtractPayload(message);
            IPAddress address = new IPAddress(payload[..sizeof(int)]);
            int port = BitConverter.ToInt32(payload, payload.Length - sizeof(int));
            return (address, port);
        }
    }
}