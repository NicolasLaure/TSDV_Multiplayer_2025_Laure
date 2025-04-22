using System.Collections.Generic;
using UnityEngine;

namespace Network.Messages
{
    public struct HandshakeResponseData
    {
        public int id;
        public int count;
        public int seed;
        public List<Vector3> positions;
    }
}
