using Health;
using Network;
using Network.Factory;
using UnityEngine;

namespace FPS.AuthServer
{
    public class AuthSvPlayerSetter : MonoBehaviour, IInstantiable
    {
        [SerializeField] private PlayerProperties _properties;
        [SerializeField] private HashHandler prefabs;

        public void SetScripts()
        {
            HealthPoints healthPoints = gameObject.AddComponent<HealthPoints>();
            PlayerController playerController = gameObject.AddComponent<PlayerController>();
            MouseLook playerLook = gameObject.AddComponent<MouseLook>();

            playerController.playerProperties = _properties;
            playerLook.playerProperties = _properties;
        }
    }
}