using System;
using CustomMath;
using FPS;
using UnityEngine;

namespace Cubes
{
    public class CubeController : MonoBehaviour
    {
        private float speed;
        private Vector3 dir;

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        private void Start()
        {
            Input.InputReader.Instance.onMove += HandleDir;
            Input.InputReader.Instance.onMove += HandleDir;
        }

        void Update()
        {
            if (dir != Vector3.zero)
            {
                transform.position += dir * speed * Time.deltaTime;
                EntityToUpdate entityToUpdate;
                entityToUpdate.gameObject = gameObject;
                entityToUpdate.trs = transform.localToWorldMatrix;
                FpsClient.Instance.onEntityUpdated?.Invoke(entityToUpdate);
            }
        }

        void HandleDir(Vec3 newDir)
        {
            dir = new Vector3(newDir.x, 0, newDir.y);
        }
    }
}