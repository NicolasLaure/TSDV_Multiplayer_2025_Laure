using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Utils
{
    public class MatrixHandler
    {
        public static Matrix4x4 Vector2To4X4(Vector2 pos)
        {
            Vector3 unityPos = new Vector3(pos.X, pos.Y);
            return Matrix4x4.Translate(unityPos);
        }
    }
}