using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputReader : MonoBehaviourSingleton<InputReader>
    {
        private PlayerInput _input;

        public Action<Vector2> onMove;

        protected override void Initialize()
        {
            _input = new PlayerInput();
            _input.Enable();

            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            onMove?.Invoke(context.ReadValue<Vector2>());
        }
    }
}