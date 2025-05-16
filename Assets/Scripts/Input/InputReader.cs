using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputReader : MonoBehaviourSingleton<InputReader>
    {
        private PlayerInput _input;

        public Action<Vector2> onMove;
        public Action<Vector2> onLook;
        public Action onCrouch;
        public Action onQuit;

        protected override void Initialize()
        {
            _input = new PlayerInput();
            _input.Enable();

            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
            _input.Player.Look.performed += OnLook;
            _input.Player.Look.canceled += OnLook;

            _input.Player.Crouch.performed += OnCrouch;

            _input.Player.Quit.performed += OnQuit;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            onMove?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            onLook?.Invoke(context.ReadValue<Vector2>());
        }

        private void OnQuit(InputAction.CallbackContext context)
        {
            onQuit?.Invoke();
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            onCrouch?.Invoke();
        }
    }
}