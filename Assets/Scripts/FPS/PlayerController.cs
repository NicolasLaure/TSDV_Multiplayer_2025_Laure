using Cubes;
using Input;
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
        FpsClient.Instance.onPlayerUpdated.Invoke(trs);
    }

    void HandleDir(Vector2 dir)
    {
        movementDir = new Vector3(dir.x, 0, dir.y);
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