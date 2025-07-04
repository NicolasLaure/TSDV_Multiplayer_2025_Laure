using MidTerm2.View;
using Unity.Mathematics;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesView : MonoBehaviour
    {
        private CastlesModel _model;

        [SerializeField] private GameObject tilesPrefab;

        public void Initialize(CastlesModel model)
        {
            _model = model;
            InitializeBoard();
        }

        public void InitializeBoard()
        {
            Instantiate(tilesPrefab);
            tilesPrefab.GetComponent<TilesManager>().SetTiles(_model.board);
        }
    }
}