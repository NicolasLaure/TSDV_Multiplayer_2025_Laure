using System;
using System.Collections.Generic;
using CustomMath;
using UnityEngine;

namespace FPS.AuthServer
{
    public class InputHandler : MonoBehaviourSingleton<InputHandler>
    {
        public Dictionary<int, Action<Vec3>> idToMoveActions = new Dictionary<int, Action<Vec3>>();
        public Dictionary<int, Action<Vec3>> idToLookActions = new Dictionary<int, Action<Vec3>>();
        public Dictionary<int, Action> idToCrouchActions = new Dictionary<int, Action>();
        public Dictionary<int, Action> idToShootActions = new Dictionary<int, Action>();
    }
}