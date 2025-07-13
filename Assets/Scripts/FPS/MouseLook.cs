using System;
using CustomMath;
using Input;
using Network;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private InputReader input;

    public PlayerProperties playerProperties;

    private Vector2 _rotationDir = Vector2.zero;
    private float xAngle;
    private float yAngle;

    private void Start()
    {
        //SetMouseLockState(true);
        xAngle = Camera.main.transform.localRotation.eulerAngles.x;
        yAngle = transform.rotation.eulerAngles.y;

        input.onLook += SetRotation;
        ClientManager.Instance.networkClient.onDisconnection += UnlockMouse;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotationDir.x * playerProperties.mouseSensitivity * Time.deltaTime);
        xAngle += _rotationDir.y * playerProperties.mouseSensitivity * Time.deltaTime;
        yAngle = Mathf.Clamp(xAngle, -playerProperties.maxVerticalRotation, playerProperties.maxVerticalRotation);
        Camera.main.transform.localRotation = Quaternion.Euler(-xAngle, 0, 0);
    }

    public void SetRotation(Vec3 dir)
    {
        _rotationDir = dir;
    }

    public void UnlockMouse()
    {
        SetMouseLockState(false);
    }

    public void SetMouseLockState(bool shouldBeLocked)
    {
        if (shouldBeLocked)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}