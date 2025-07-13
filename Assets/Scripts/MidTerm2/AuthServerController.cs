using System;
using Network.Factory;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class AuthServerController : MonoBehaviourSingleton<AuthServerController>
    {
        public CastlesModel model;
        public ReflectionHandler<CastlesModel> reflection;
        public ReflectiveServerFactory<CastlesModel> factory;
        private PlayerInput playerInput;

        public void StartUp()
        {
            CastlesAuthServer.Instance.onMouseClick += HandleClick;
        }

        public void HandleClick(Tuple<Vector2, int> posId)
        {
            if ((model.isPlayerOneTurn && posId.Item2 != 0) || (!model.isPlayerOneTurn && posId.Item2 == 0))
                return;

            Debug.Log($"Player: {posId.Item2}, Clicked at: {posId.Item1}");
            // Mouse.current.WarpCursorPosition(pos);
            // Mouse.current.press.
        }
    }
}