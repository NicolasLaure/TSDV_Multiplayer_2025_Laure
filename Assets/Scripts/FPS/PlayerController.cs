using System;
using System.Collections;
using System.Collections.Generic;
using Cubes;
using CustomMath;
using FPS;
using FPS.Bullet;
using Health;
using Input;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader input;

    public PlayerProperties playerProperties;
    protected Vector3 movementDir;

    protected bool isCrouching = false;
    protected Matrix4x4 lastFrameTrs;
    protected HealthPoints _healthPoints;

    protected virtual void Start()
    {
        input.onMove += HandleDir;
        input.onCrouch += ToggleCrouch;
        input.onShoot += Shoot;

        lastFrameTrs = transform.localToWorldMatrix;

        _healthPoints = GetComponent<HealthPoints>();
        _healthPoints.onDeathEvent += OnDeath;
    }

    protected void OnDestroy()
    {
        if (Camera.main != null)
            Camera.main.transform.parent = null;

        input.onMove -= HandleDir;
        input.onCrouch -= ToggleCrouch;
        input.onShoot -= Shoot;

        _healthPoints.onDeathEvent -= OnDeath;
    }

    protected void Update()
    {
        Move();

        if (lastFrameTrs != transform.localToWorldMatrix)
            SendActualPosition();

        lastFrameTrs = transform.localToWorldMatrix;
    }

    protected void Move()
    {
        if (movementDir != Vector3.zero && !isCrouching)
        {
            transform.Translate(movementDir * (playerProperties.speed * Time.deltaTime));
        }
    }

    protected virtual void SendActualPosition()
    {
        Matrix4x4 trs = transform.localToWorldMatrix;
        EntityToUpdate thisEntity;
        thisEntity.gameObject = gameObject;
        thisEntity.trs = trs;
        FpsClient.Instance.onEntityUpdated.Invoke(thisEntity);
    }

    protected void HandleDir(Vec3 dir)
    {
        movementDir = new Vector3(dir.x, 0, dir.y);
    }

    protected virtual void Shoot()
    {
        Vector3 bulletSpawnPosition = transform.right * playerProperties.shootingPoint.x + transform.up * playerProperties.shootingPoint.y + transform.forward * playerProperties.shootingPoint.z;
        Matrix4x4 bulletSpawnTrs = Matrix4x4.TRS(transform.position + bulletSpawnPosition, Camera.main.transform.rotation, Vector3.one);
        FpsClient.Instance.SendInstantiateRequest(playerProperties.bulletPrefab, bulletSpawnTrs, (short)Colors.Black);
    }

    protected virtual void ToggleCrouch()
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

    protected void SetPlayerHeight(float yPos, float yScale)
    {
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
        transform.localScale = new Vector3(1, yScale, 1);
    }

    protected void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hitted");
        if (other.TryGetComponent<BulletSetter>(out BulletSetter bulletObject) && !other.TryGetComponent<BulletController>(out BulletController bulletController))
        {
            int damageTaken = isCrouching ? bulletObject.properties.damage / 2 : bulletObject.properties.damage;
            HealthPoints playerHealth = GetComponent<HealthPoints>();
            playerHealth.TryTakeDamage(damageTaken);
            playerProperties.onPlayerTakeDamage.RaiseEvent(playerHealth.CurrentHp);
        }
    }

    public void OnDeath()
    {
        FpsClient.Instance.SendDeath();
    }
}