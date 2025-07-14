using MidTerm2.View;
using Network.Factory;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesController : MonoBehaviourSingleton<CastlesController>
    {
        public CastlesModel model;
        public ReflectiveFactory<CastlesModel> factory;
        public ReflectionHandler<CastlesModel> reflection;

        public void NextTurn()
        {
            model?.ChangeTurn();
        }

        public void SelectTile(GameObject tileObject)
        {
            if (factory == null || model == null || !model.isPlayerOneTurn)
                return;

            TileView tileView = tileObject.GetComponent<TileView>();
            if (tileView.tileObject != null && tileView.tileObject.TryGetComponent<TileObjectView>(out TileObjectView tileObjView))
            {
                if (!factory.TryGetInstanceId(tileView.tileObject, out int instanceId, out int originalClientId)) return;

                if (originalClientId == CastlesClient.Instance.clientId && tileObjView != null)
                {
                    model.SelectTileObject(tileView.position, CastlesClient.Instance.clientId);
                }
            }
            else
                model.SelectTile(tileView.position, CastlesClient.Instance.clientId);
        }
    }
}