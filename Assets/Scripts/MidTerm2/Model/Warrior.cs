using System;
using Network.Utilities;
using UnityEngine;
using Random = System.Random;

namespace MidTerm2.Model
{
    [Serializable]
    public class Warrior : TileObject
    {
        public Warrior() : base(50)
        {
        }

        public Warrior(int maxHealth) : base(maxHealth)
        {
        }

        public void Move(Tile targetTile, ref int remainingMoves)
        {
            if (targetTile.currentObject != null)
                return;

            Vector2 offset = targetTile.position - position;
            int moves = (int)MathF.Abs(offset.x) + (int)MathF.Abs(offset.y);

            if (moves <= remainingMoves)
            {
                remainingMoves -= moves;
                SetTile(targetTile);
            }
        }

        public void Attack(TileObject target)
        {
            Random random = new Random((int)ServerTime.time);
            target.TakeDamage((int)(currentHealth * 0.2f) + 5 + random.Next(2, 8));
        }
    }
}