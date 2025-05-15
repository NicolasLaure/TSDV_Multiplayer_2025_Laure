using System;
using Cubes;
using Input;
using Network;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Vector3 movementDir;

    void Start()
    {
        InputReader.Instance.onMove += HandleDir;
    }

    void Update()
    {
        Move();

        SendActualPosition();
    }

    private void Move()
    {
        if (movementDir != Vector3.zero)
        {
            transform.Translate(movementDir * (speed * Time.deltaTime));
        }
    }

    private void SendActualPosition()
    {
        Matrix4x4 trs = transform.localToWorldMatrix;
        FpsClient.Instance.onPlayerUpdated.Invoke(trs);
    }

    void HandleDir(Vector2 dir)
    {
        movementDir = new Vector3(dir.x, 0, dir.y);
    }
}