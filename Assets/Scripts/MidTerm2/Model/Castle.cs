using System;

namespace MidTerm2.Model
{
    [Serializable]
    public class Castle : TileObject
    {
        public Castle(int maxHealthPoints) : base(maxHealthPoints)
        {
        }
    }
}