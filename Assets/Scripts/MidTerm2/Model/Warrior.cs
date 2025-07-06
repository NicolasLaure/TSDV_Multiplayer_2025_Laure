using System;

namespace MidTerm2.Model
{
    [Serializable]
    public class Warrior : TileObject
    {
        public Warrior() : base(0)
        {
        }

        public Warrior(int maxHealth) : base(maxHealth)
        {
        }
    }
}