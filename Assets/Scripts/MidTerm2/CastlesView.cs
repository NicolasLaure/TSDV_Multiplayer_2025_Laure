using MidTerm2.Model;
using MidTerm2.View;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesView : MonoBehaviourSingleton<CastlesView>
    {
        private CastlesModel _model;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject castlePrefab;
        [SerializeField] private GameObject warriorPrefab;

        [SerializeField] private float tileOffset;

        public void InitializeView(CastlesModel model)
        {
            _model = model;
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            Instantiate(tilePrefab);
            SetTiles(_model.board);

            //GameObject castleGO = Instantiate(castlePrefab);
            //TileObjectView castleView = castleGO.GetComponent<TileObjectView>();
            //castleView.SetPosition(_model.board[(int)_model._castle.position.X][(int)_model._castle.position.Y]);

            // foreach (Warrior warrior in _model._warriors)
            // {
            //     GameObject warriorGO = Instantiate(warriorPrefab);
            //     TileObjectView view = warriorGO.GetComponent<TileObjectView>();
            //     view.SetPosition(_model.board[(int)warrior.position.X][(int)warrior.position.Y]);
            //     warrior.onMove += view.SetPosition;
            // }
        }


        public void SetTiles(Tile[][] board)
        {
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board.Length; j++)
                {
                    GameObject tile = Instantiate(tilePrefab);
                    tile.transform.position = IndexToPosition(new Vector2(i, j), 0.1f);
                }
            }
        }

        public Vector3 IndexToPosition(Vector2 indexPosition, float depthOffset)
        {
            float appliedOffset = tilePrefab.transform.localScale.x + tileOffset;
            float xPos = (indexPosition.x - _model.board.Length / 2) * appliedOffset;
            float yPos = (_model.board.Length / 2 - indexPosition.y) * appliedOffset;
            return new Vector3(xPos, yPos, depthOffset);
        }
    }
}