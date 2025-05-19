using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPS.AuthServer
{
    public class InputHandler : MonoBehaviourSingleton<InputHandler>
    {
        public Dictionary<int, Action<Vector2>> idToMoveActions = new Dictionary<int, Action<Vector2>>();
        public Dictionary<int, Action<Vector2>> idToLookActions = new Dictionary<int, Action<Vector2>>();
        public Dictionary<int, Action> idToCrouchActions = new Dictionary<int, Action>();
        public Dictionary<int, Action> idToShootActions = new Dictionary<int, Action>();
    }
}