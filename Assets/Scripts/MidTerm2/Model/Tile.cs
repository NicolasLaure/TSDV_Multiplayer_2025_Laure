using System.Numerics;

namespace MidTerm2.Model
{
    public class Tile
    {
        public TileObject currentObject = null;
        public Vector2 position;

        public Tile(Vector2 pos)
        {
            position = pos;
        }
    }
}