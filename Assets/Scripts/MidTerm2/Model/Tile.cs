
using UnityEngine;

namespace MidTerm2.Model
{
    public class Tile
    {
        public TileObject currentObject = null;
        public Vector2 position;
        public bool isTaken;

        public Tile(Vector2 pos)
        {
            position = pos;
        }
    }
}