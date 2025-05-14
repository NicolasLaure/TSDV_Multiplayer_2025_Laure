using System.Collections.Generic;
using System.Numerics;

namespace Network.Messages
{
    public struct ServerHandshakeResponseData
    {
        public int id;
        public int seed;
        public int count;
        public List<(bool isActive, byte[] position)> cubes;
    }
}
