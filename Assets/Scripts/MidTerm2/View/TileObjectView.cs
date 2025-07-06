using MidTerm2.Model;
using UnityEngine;

namespace MidTerm2.View
{
    public class TileObjectView : MonoBehaviour
    {
        public Vector2 pos;

        public void SetPosition(Tile newTile)
        {
            pos = new Vector2(newTile.position.X, newTile.position.Y);
            transform.position = CastlesView.Instance.IndexToPosition(pos, 0);
        }
    }
}