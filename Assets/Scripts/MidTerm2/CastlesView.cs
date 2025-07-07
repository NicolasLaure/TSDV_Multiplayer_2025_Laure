using MidTerm2.Model;
using MidTerm2.View;
using Network.Factory;
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

        [Header("UI")]
        [SerializeField] private GameObject passTurnButton;
        [SerializeField] private RemainingMoves movesText;

        public TileView[][] boardView;

        public void InitializeView(CastlesModel model)
        {
            _model = model;
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            Instantiate(tilePrefab);
            SetTiles(_model.board);
        }

        private void Update()
        {
            if (_model == null)
                return;

            if (passTurnButton.activeInHierarchy != _model.isPlayerTurn)
                OnTurnChanged();
        }

        public void SetTileObjectPosition(ObjectModel tileObject)
        {
            TileObjectView objectView = tileObject.view.GetComponent<TileObjectView>();
            ((TileObject)tileObject.obj).onMove += objectView.SetPosition;
        }

        public void SetTiles(Tile[][] board)
        {
            boardView = new TileView[board.Length][];
            for (int i = 0; i < board.Length; i++)
            {
                boardView[i] = new TileView[board.Length];
                for (int j = 0; j < board.Length; j++)
                {
                    GameObject tile = Instantiate(tilePrefab);
                    tile.transform.position = IndexToPosition(new Vector2(i, j), 0.1f);
                    boardView[i][j] = tile.GetComponent<TileView>();
                    boardView[i][j].position = new System.Numerics.Vector2(i, j);
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

        private void OnTurnChanged()
        {
            passTurnButton.SetActive(_model.isPlayerTurn);
            movesText.SetText(_model.remainingMoves);
        }
    }
}