using MidTerm2.Model;
using UnityEngine;

namespace MidTerm2.View
{
    public class TilesManager : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;

        [SerializeField] private float offset = 0.1f;

        private void Start()
        {
            transform.position = Camera.main.ViewportToWorldPoint(new Vector3(-0.35f, -1));
        }

        public void SetTiles(Tile[][] board)
        {
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board.Length; j++)
                {
                    GameObject tile = Instantiate(tilePrefab);
                    float appliedOffset = tile.transform.localScale.x + offset;
                    float xPos = (i - board.Length / 2) * appliedOffset;
                    float yPos = (board.Length / 2 - j) * appliedOffset;

                    tile.transform.position = new Vector3(xPos, yPos);
                }
            }
        }
    }
}