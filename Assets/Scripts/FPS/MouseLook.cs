using System;
using Input;
using Network;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity;
    public float maxVerticalRotation;

    private Vector2 _rotationDir = Vector2.zero;
    private float xAngle;
    private float yAngle;

    private void Start()
    {
        //SetMouseLockState(true);
        xAngle = Camera.main.transform.localRotation.eulerAngles.x;
        yAngle = transform.rotation.eulerAngles.y;

        InputReader.Instance.onLook += SetRotation;
        //ClientManager.Instance.networkClient.onDisconnection += UnlockMouse;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotationDir.x * mouseSensitivity * Time.deltaTime);
        xAngle += _rotationDir.y * mouseSensitivity * Time.deltaTime;
        yAngle = Mathf.Clamp(xAngle, -maxVerticalRotation, maxVerticalRotation);
        Camera.main.transform.localRotation = Quaternion.Euler(-xAngle, 0, 0);
    }

    public void SetRotation(Vector2 dir)
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