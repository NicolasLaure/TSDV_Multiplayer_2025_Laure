using System.Collections.Generic;
using Network;
using Network.Factory;
using UnityEngine;

namespace FPS
{
    public class PlayerSetter : MonoBehaviour, IInstantiable
    {
        [SerializeField] private PlayerProperties _properties;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private HashHandler prefabs;

        public void SetData(InstanceData data)
        {
            Matrix4x4 trs = ByteFormat.Get4X4FromBytes(data.trs, 0);
            transform.position = trs.GetPosition();
            transform.rotation = trs.rotation;

            PlayerController playerController = gameObject.AddComponent<PlayerController>();
            MouseLook playerLook = gameObject.AddComponent<MouseLook>();

            playerController.playerProperties = _properties;
            playerLook.playerProperties = _properties;

            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = _properties.cameraOffset;
        }

        public void SetScripts()
        {
            throw new System.NotImplementedException();
        }
    }
}