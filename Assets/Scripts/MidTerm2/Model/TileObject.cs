using System;
using System.Numerics;
using Reflection;

namespace MidTerm2.Model
{
    [Serializable]
    public class TileObject
    {
        public event Action onDeath;
        public event Action<Tile> onMove;
        [Sync] protected int currentHealth;
        [Sync] public Vector2 position;

        public TileObject(int maxHealth)
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
                onDeath?.Invoke();
        }

        public void SetTile(Tile newTile)
        {
            onMove?.Invoke(newTile);
            position = newTile.position;
        }
    }
}