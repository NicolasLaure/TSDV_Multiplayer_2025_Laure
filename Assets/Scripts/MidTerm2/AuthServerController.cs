using System;
using MidTerm2.View;
using Network.Factory;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class AuthServerController : MonoBehaviourSingleton<AuthServerController>
    {
        [SerializeField] private Transform cursorPos;
        public CastlesModel model;
        public ReflectionHandler<CastlesModel> reflection;
        public ReflectiveServerFactory<CastlesModel> factory;
        private PlayerInput playerInput;

        public void StartUp()
        {
            CastlesAuthServer.Instance.onMouseClick += HandleClick;
        }

        public void NextTurn()
        {
            model?.ChangeTurn();
        }

        public void SelectTile(GameObject tileObject, int clientId)
        {
            if (factory == null || model == null)
                return;

            TileView tileView = tileObject.GetComponent<TileView>();
            if (tileView.tileObject != null && tileView.tileObject.TryGetComponent(out TileObjectView tileObjView))
            {
                if (!factory.TryGetInstanceId(tileView.tileObject, out int instanceId, out int originalClientId))
                {
                    Debug.Log("InstanceId not found TileObjectSelected");
                    return;
                }

                Debug.Log($"ClientClick: {clientId}, tileObject ID:{originalClientId}");
                if (originalClientId == clientId && tileObjView != null)
                {
                    Debug.Log("TileObjectSelected");
                    model.SelectTileObject(tileView.position, clientId);
                }
            }
            else
                model.SelectTile(tileView.position, clientId);
        }

        public void HandleClick(Tuple<Vector2, int> posId)
        {
            if ((model.isPlayerOneTurn && posId.Item2 != 0) || (!model.isPlayerOneTurn && posId.Item2 == 0))
                return;

            Debug.Log($"Player: {posId.Item2}, Clicked at: {posId.Item1}");

            cursorPos.position = posId.Item1;

            Ray ray = new Ray(new Vector3(cursorPos.position.x, cursorPos.position.y, -10), Vector3.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"Hit: {hit.transform.name}");
                if (hit.transform.gameObject.TryGetComponent(out TileView tileView))
                {
                    Debug.Log("IS TILE");
                    SelectTile(tileView.gameObject, posId.Item2);
                }
                else if (hit.collider.CompareTag("Button"))
                    NextTurn();
            }
            else
                Debug.Log($"No Object Hit");
        }
    }
}