using UnityEngine;

namespace FPS.Bullet
{
    [CreateAssetMenu(fileName = "BulletProperties", menuName = "Bullet", order = 0)]
    public class BulletProperties : ScriptableObject
    {
        public float speed;
        public float bulletLife;
        public int damage;
    }
}