using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public struct HandshakeResponseData
    {
        public int id;
        public int seed;
        public int count;
        public List<Vector3> positions;
    }
}
