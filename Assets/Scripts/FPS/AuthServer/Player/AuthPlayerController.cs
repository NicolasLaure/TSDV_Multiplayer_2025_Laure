using Health;
using UnityEngine;

namespace FPS.AuthServer
{
    public class AuthPlayerController : PlayerController
    {
        public int id;

        protected override void Start()
        {
            InputHandler.Instance.idToMoveActions[id] += HandleDir;
            InputHandler.Instance.idToCrouchActions[id] += ToggleCrouch;
            InputHandler.Instance.idToShootActions[id] += Shoot;

            lastFrameTrs = transform.localToWorldMatrix;

            _healthPoints = GetComponent<HealthPoints>();
            _healthPoints.onDeathEvent += OnDeath;
        }

        protected override void SendActualPosition()
        {
            Matrix4x4 trs = transform.localToWorldMatrix;
            EntityToUpdate thisEntity;
            thisEntity.gameObject = gameObject;
            thisEntity.trs = trs;
           // FpsServer.Instance.onEntityUpdated.Invoke((thisEntity, id));
        }

        protected override void Shoot()
        {
            Vector3 bulletSpawnPosition = transform.right * playerProperties.shootingPoint.x + transform.up * playerProperties.shootingPoint.y + transform.forward * playerProperties.shootingPoint.z;
            Matrix4x4 bulletSpawnTrs = Matrix4x4.TRS(transform.position + bulletSpawnPosition, Camera.main.transform.rotation, Vector3.one);
           // FpsServer.Instance.Instantiate(playerProperties.bulletPrefab, bulletSpawnTrs, (short)Colors.Black, id);
        }

        protected override void ToggleCrouch()
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                SetPlayerHeight(playerProperties.crouchYPosition, playerProperties.crouchSize);
            }
            else
            {
                SetPlayerHeight(playerProperties._defaultYPosition, playerProperties._defaultSize);
            }

            SendActualPosition();
        }
    }
}