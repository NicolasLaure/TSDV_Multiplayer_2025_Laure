using UnityEngine;

namespace FPS
{
    public class TransformHandler : MonoBehaviour
    {
        public void SetHeight(float sizeY, float positionY)
        {
            transform.position = new Vector3(transform.position.x, positionY, transform.position.z);
            transform.localScale = new Vector3(1, sizeY, 1);
        }
    }
}