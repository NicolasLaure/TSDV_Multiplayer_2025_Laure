using Network;
using UnityEngine;

namespace FPS.Bullet
{
    public class BulletSetter : MonoBehaviour, IInstantiable
    {
        [SerializeField] private BulletProperties properties;

        public void SetScripts()
        {
            BulletController bulletController = gameObject.AddComponent<BulletController>();
            bulletController.bulletProperties = properties;
        }
    }
}