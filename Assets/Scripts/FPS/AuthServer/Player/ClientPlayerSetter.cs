using Network;
using UnityEngine;

namespace FPS.AuthServer.Player
{
    public class ClientPlayerSetter : MonoBehaviour, IInstantiable
    {
        [SerializeField] private PlayerProperties _properties;
        [SerializeField] private GameObject cameraPrefab;

        public void SetScripts()
        {
            if (Camera.main == null)
            {
                Instantiate(cameraPrefab);
            }

            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = _properties.cameraOffset;
        }
    }
}