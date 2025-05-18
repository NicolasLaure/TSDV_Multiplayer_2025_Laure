using System;
using Cubes;
using FPS;
using Input;
using Network;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerProperties playerProperties;
    private Vector3 movementDir;

    private bool isCrouching = false;

    void Start()
    {
        InputReader.Instance.onMove += HandleDir;
        InputReader.Instance.onCrouch += ToggleCrouch;
        InputReader.Instance.onShoot += Shoot;
    }

    void Update()
    {
        Move();

        SendActualPosition();
    }

    private void Move()
    {
        if (movementDir != Vector3.zero && !isCrouching)
        {
            transform.Translate(movementDir * (playerProperties.speed * Time.deltaTime));
        }
    }

    private void SendActualPosition()
    {
        Matrix4x4 trs = transform.localToWorldMatrix;
        EntityToUpdate thisEntity;
        thisEntity.gameObject = gameObject;
        thisEntity.trs = trs;
        FpsClient.Instance.onPlayerUpdated.Invoke(thisEntity);
    }

    void HandleDir(Vector2 dir)
    {
        movementDir = new Vector3(dir.x, 0, dir.y);
    }

    private void Shoot()
    {
        Vector3 bulletSpawnPosition = transform.right * playerProperties.shootingPoint.x + transform.up * playerProperties.shootingPoint.y + transform.forward * playerProperties.shootingPoint.z;
        Matrix4x4 bulletSpawnTrs = Matrix4x4.TRS(transform.position + bulletSpawnPosition, Camera.main.transform.rotation, Vector3.one);
        FpsClient.Instance.SendInstantiateRequest(playerProperties.bulletPrefab, bulletSpawnTrs, (short)Colors.Black);
    }

    void ToggleCrouch()
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

        FpsClient.Instance.onCrouch?.Invoke(isCrouching);
    }

    void SetPlayerHeight(float yPos, float yScale)
    {
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
        transform.localScale = new Vector3(1, yScale, 1);
    }
}