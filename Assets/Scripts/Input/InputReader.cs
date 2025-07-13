using System;
using CustomMath;
using MidTerm2;
using Network.Messages;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputReader : MonoBehaviour
    {
        private PlayerInput _input;

        public Action<Vec3> onMove;
        public Action<Vec3> onLook;
        public Action onShoot;
        public Action onCrouch;
        public Action onQuit;

        public Action<bool> onPingScreen;

        public void Initialize()
        {
            _input = new PlayerInput();
            _input.Enable();

            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
            _input.Player.Look.performed += OnLook;
            _input.Player.Look.canceled += OnLook;

            _input.Player.Fire.performed += OnShoot;
            _input.Player.Crouch.performed += OnCrouch;

            _input.Player.Quit.performed += OnQuit;

            _input.Player.PingScreen.started += OnPingScreen;
            _input.Player.PingScreen.canceled += OnPingScreen;

            _input.Player.MouseClick.started += OnMouseClick;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            onMove?.Invoke(new Vec3(context.ReadValue<Vector2>()));
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            onLook?.Invoke(new Vec3(context.ReadValue<Vector2>()));
        }

        private void OnQuit(InputAction.CallbackContext context)
        {
            onQuit?.Invoke();
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            onShoot?.Invoke();
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            onCrouch?.Invoke();
        }

        private void OnPingScreen(InputAction.CallbackContext context)
        {
            if (context.started)
                onPingScreen?.Invoke(true);
            else if (context.canceled)
                onPingScreen?.Invoke(false);
        }

        private void OnMouseClick(InputAction.CallbackContext context)
        {
            Debug.Log($"Click at: {context.ReadValue<Vector2>()}");
            MouseInput input;
            Vector2 pos = context.ReadValue<Vector2>();
            input.x = pos.x;
            input.y = pos.y;
            if (CastlesNaClient.Instance != null)
                CastlesNaClient.Instance.networkClient.SendToServer(new MouseClickMessage(input).Serialize());
        }
    }
}