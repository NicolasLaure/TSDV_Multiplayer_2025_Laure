using System;

namespace MidTerm2.Model
{
    [Serializable]
    public class Castle : TileObject
    {
        public Castle() : base(0)
        {
        }

        public Castle(int maxHealthPoints) : base(maxHealthPoints)
        {
        }
    }
}