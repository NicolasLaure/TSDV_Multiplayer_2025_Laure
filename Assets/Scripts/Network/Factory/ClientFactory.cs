using System.Collections.Generic;
using Cubes;
using Network.Messages;
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
            Matrix4x4 trs = ByteFormat.Get4X4FromBytes(instanceData.trs, 0);
            GameObject instance = GameObject.Instantiate(prefab, trs.GetPosition(), trs.rotation);
            if (instanceData.originalClientID == FpsClient.Instance.clientId)
            {
                instance.GetComponent<IInstantiable>().SetData(instanceData);
                instance.GetComponent<IInstantiable>().SetScripts();
            }

            SaveGameObject(instanceData.instanceID, instance);
        }

        public void DeInstantiate(int instanceId)
        {
            RemoveGameObject(instanceId, instanceIdToGameObject[instanceId]);
            GameObject.Destroy(instanceIdToGameObject[instanceId]);
        }

        public void InstantiateMultiple(InstantiateAll objectsToInstantiate)
        {
            for (int i = 0; i < objectsToInstantiate.count; i++)
            {
                GameObject prefab = prefabsData.hashToPrefab[objectsToInstantiate.instancesData[i].prefabHash];
                Matrix4x4 trs = ByteFormat.Get4X4FromBytes(objectsToInstantiate.instancesData[i].trs, 0);
                GameObject instance = GameObject.Instantiate(prefab, trs.GetPosition(), trs.rotation);
                SaveGameObject(objectsToInstantiate.instancesData[i].instanceID, instance);
            }
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