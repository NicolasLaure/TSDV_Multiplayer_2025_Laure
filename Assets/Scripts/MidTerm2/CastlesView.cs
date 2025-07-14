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
        private int _clientId = -1;

        public void InitializeView(CastlesModel model, int clientId)
        {
            Debug.Log($"ClientID:{clientId}");
            _clientId = clientId;
            _model = model;
            InitializeBoard();
            _model.onTurnChange += OnTurnChanged;
        }

        public void InitializeBoard()
        {
            Instantiate(tilePrefab);
            SetTiles(_model.board);
            if (CastlesNaClient.Instance != null)
                _clientId = CastlesNaClient.Instance.clientId;
            else if (CastlesClient.Instance != null)
                _clientId = CastlesClient.Instance.clientId;

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
                    boardView[i][j].position = new Vector2(i, j);
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
            bool isPlayerOne = _clientId == 0;
            if (passTurnButton == null || movesText == null)
                return;

            Debug.Log($"Client ID:{_clientId}, isPlayerOneTurn: {_model.isPlayerOneTurn}, shouldShowButton: {(isPlayerOne && _model.isPlayerOneTurn) || (!isPlayerOne && !_model.isPlayerOneTurn)}");
            passTurnButton.SetActive((isPlayerOne && _model.isPlayerOneTurn) || (!isPlayerOne && !_model.isPlayerOneTurn));
            movesText.SetText(_model.remainingMoves);
        }
    }
}