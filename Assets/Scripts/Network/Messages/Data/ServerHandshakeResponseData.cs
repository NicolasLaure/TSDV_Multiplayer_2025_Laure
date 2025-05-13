using System.Collections.Generic;
using Cubes;

namespace Network.Messages
{
    public struct ServerHandshakeResponseData
    {
        public int id;
        public int seed;
        public int count;
        public List<Cube> cubes;
    }
}
