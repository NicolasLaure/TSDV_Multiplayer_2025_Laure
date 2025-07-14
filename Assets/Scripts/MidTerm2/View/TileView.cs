using UnityEngine;

namespace MidTerm2.View
{
    public class TileView : MonoBehaviour
    {
        public Vector2 position;
        public GameObject tileObject = null;

        private void OnMouseDown()
        {
            if (CastlesController.Instance == null)
                return;

            CastlesController.Instance.SelectTile(gameObject);
        }
    }
}