using System.Net;

namespace Network
{
    public class MatchmakerManager : NetworkManager<MatchmakerManager>
    {
        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            throw new System.NotImplementedException();
        }
    }
}