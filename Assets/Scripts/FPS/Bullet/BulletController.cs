using System.Collections;
using Cubes;
using UnityEngine;

namespace FPS.Bullet
{
    public class BulletController : MonoBehaviour
    {
        public BulletProperties bulletProperties;
        private Coroutine _timedDeath;
        private float delayAfterContact = 0.05f;

        private void Start()
        {
            _timedDeath = StartCoroutine(DelayedDestroy(bulletProperties.bulletLife));
        }

        void Update()
        {
            transform.Translate(Vector3.forward * (bulletProperties.speed * Time.deltaTime));

            EntityToUpdate thisEntity;
            thisEntity.gameObject = gameObject;
            thisEntity.trs = transform.localToWorldMatrix;
            FpsClient.Instance.onEntityUpdated.Invoke(thisEntity);
        }

        private void OnTriggerEnter(Collider other)
        {
            StartCoroutine(DelayedDestroy(delayAfterContact));
        }

        void ShouldDestroy()
        {
            FpsClient.Instance.SendDeInstantiateRequest(gameObject);
        }

        private IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShouldDestroy();
        }
    }
}