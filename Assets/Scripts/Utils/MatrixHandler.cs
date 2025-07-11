using UnityEngine;

namespace Utils
{
    public class MatrixHandler
    {
        public static Matrix4x4 Vector2To4X4(Vector2 pos)
        {
            Vector3 unityPos = new Vector3(pos.x, pos.y);
            return Matrix4x4.Translate(unityPos);
        }
    }
}