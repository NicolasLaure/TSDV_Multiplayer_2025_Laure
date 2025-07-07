using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace MidTerm2.View
{
    public class TileView : MonoBehaviour
    {
        public Vector2 position;
        public GameObject tileObject = null;


        private void OnMouseDown()
        {
            CastlesController.Instance.SelectTile(gameObject);
        }
    }
}