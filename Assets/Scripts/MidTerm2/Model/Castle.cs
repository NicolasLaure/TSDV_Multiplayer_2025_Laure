using System;

namespace MidTerm2.Model
{
    public class Castle : ITileObject
    {
        public int posX;
        public int posY;
        private int currentHp;
        private Action onDeath;

        public Castle(int maxHealthPoints)
        {
            currentHp = maxHealthPoints;
        }

        public void TakeDamage(int damage)
        {
            currentHp -= damage;
            if (currentHp <= 0)
                onDeath?.Invoke();
        }
    }
}