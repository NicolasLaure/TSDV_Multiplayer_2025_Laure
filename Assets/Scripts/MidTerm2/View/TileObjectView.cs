using MidTerm2.Model;
using UnityEngine;

namespace MidTerm2.View
{
    public class TileObjectView : MonoBehaviour
    {
        public Vector2 pos;
        public TileObject tileObject;

        public void SetPosition(Tile newTile)
        {
            pos = new Vector2(newTile.position.x, newTile.position.y);
            transform.position = CastlesView.Instance.IndexToPosition(pos, 0);
            CastlesView.Instance.boardView[(int)newTile.position.x][(int)newTile.position.y].tileObject = gameObject;
        }
    }
}