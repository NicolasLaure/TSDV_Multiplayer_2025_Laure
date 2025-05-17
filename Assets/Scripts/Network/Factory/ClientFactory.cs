using System.Collections.Generic;
using UnityEngine;

namespace Network.Factory
{
    public class ClientFactory : NetworkFactory
    {
        private readonly Dictionary<int, GameObject> instanceIdToGameObject = new Dictionary<int, GameObject>();

        private HashHandler prefabsData;

        public ClientFactory(HashHandler gameObjectsData)
        {
            prefabsData = gameObjectsData;
        }

        public void Instantiate(InstanceData instanceData)
        {
            GameObject prefab = prefabsData.hashToPrefab[instanceData.prefabHash];
            GameObject instance = GameObject.Instantiate(prefab);
            SaveGameObject(instanceData.instanceID, instance);
        }

        public void DeInstantiate(int instanceId)
        {
            RemoveGameObject(instanceId, instanceIdToGameObject[instanceId]);
            GameObject.Destroy(instanceIdToGameObject[instanceId]);
        }

        private void SaveGameObject(int instanceId, GameObject gameObject)
        {
            instanceIdToGameObject[instanceId] = gameObject;
        }

        private void RemoveGameObject(int instanceId, GameObject gameObject)
        {
            if (instanceIdToGameObject.ContainsKey(instanceId))
                instanceIdToGameObject.Remove(instanceId);
        }
    }
}