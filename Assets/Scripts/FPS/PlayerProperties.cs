using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProperties", menuName = "Player/Properties", order = 0)]
public class PlayerProperties : ScriptableObject
{
    [Header("PlayerMovement")]
    public float speed = 5;

    [Header("MouseLook")]
    public Vector3 cameraOffset = new Vector3(0, 0.5f, 0.2f);
    public float mouseSensitivity = 5;
    public float maxVerticalRotation = 80;
}