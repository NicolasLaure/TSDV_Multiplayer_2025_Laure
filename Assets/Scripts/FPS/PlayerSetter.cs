using System;
using System.Collections.Generic;
using Network;
using Network.Factory;
using UnityEngine;

namespace FPS
{
    public class PlayerSetter : MonoBehaviour, IInstantiable
    {
        [SerializeField] private PlayerProperties _properties;
        [SerializeField] private HashHandler prefabs;
        [SerializeField] private GameObject cameraPrefab;

        public void SetScripts()
        {
            PlayerController playerController = gameObject.AddComponent<PlayerController>();
            MouseLook playerLook = gameObject.AddComponent<MouseLook>();

            playerController.playerProperties = _properties;
            playerLook.playerProperties = _properties;

            if (Camera.main == null)
            {
                GameObject.Instantiate(cameraPrefab);
            }

            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = _properties.cameraOffset;
        }
    }
}