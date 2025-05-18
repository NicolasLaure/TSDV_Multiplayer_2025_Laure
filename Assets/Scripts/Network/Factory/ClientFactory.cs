using System.Collections.Generic;
using Cubes;
using Network.Messages;
using UnityEngine;

namespace Network.Factory
{
    public class ClientFactory : NetworkFactory
    {
        private readonly Dictionary<int, GameObject> instanceIdToGameObject = new Dictionary<int, GameObject>();
        private readonly Dictionary<GameObject, int> gameObjectToId = new Dictionary<GameObject, int>();

        private HashHandler prefabsData;

        public ClientFactory(HashHandler gameObjectsData)
        {
            prefabsData = gameObjectsData;
        }

        public void Instantiate(InstanceData instanceData)
        {
            if (!prefabsData.hashToPrefab.ContainsKey(instanceData.prefabHash))
            {
                Debug.Log("Couldn't Find prefab");
                return;
            }

            GameObject prefab = prefabsData.hashToPrefab[instanceData.prefabHash];
            Matrix4x4 trs = ByteFormat.Get4X4FromBytes(instanceData.trs, 0);
            GameObject instance = GameObject.Instantiate(prefab, trs.GetPosition(), trs.rotation);
            if (instanceData.originalClientID == FpsClient.Instance.clientId)
                instance.GetComponent<IInstantiable>().SetScripts();

            SaveGameObject(instanceData, instance);

            FpsClient.Instance.SendIntegrityCheck(instanceData);
        }

        public void DeInstantiate(int instanceId)
        {
            GameObject gameObjectToDestroy = instanceIdToGameObject[instanceId];
            RemoveGameObject(instanceId, instanceIdToGameObject[instanceId]);
            GameObject.Destroy(gameObjectToDestroy);
        }

        public void InstantiateMultiple(InstantiateAll objectsToInstantiate)
        {
            for (int i = 0; i < objectsToInstantiate.count; i++)
            {
                GameObject prefab = prefabsData.hashToPrefab[objectsToInstantiate.instancesData[i].prefabHash];
                Matrix4x4 trs = ByteFormat.Get4X4FromBytes(objectsToInstantiate.instancesData[i].trs, 0);
                GameObject instance = GameObject.Instantiate(prefab, trs.GetPosition(), trs.rotation);
                SaveGameObject(objectsToInstantiate.instancesData[i], instance);
            }
        }

        public void DeInstantiateAll()
        {
            List<GameObject> allGameObjects = GetAllGameObjects();
            for (int i = 0; i < allGameObjects.Count; i++)
            {
                RemoveGameObject(gameObjectToId[allGameObjects[i]], allGameObjects[i]);
                GameObject.Destroy(allGameObjects[i]);
            }
        }

        private void SaveGameObject(InstanceData instanceData, GameObject gameObject)
        {
            instanceIdToInstanceData[instanceData.instanceID] = instanceData;
            instanceIdToGameObject[instanceData.instanceID] = gameObject;
            gameObjectToId[gameObject] = instanceData.instanceID;
        }

        private void RemoveGameObject(int instanceId, GameObject gameObject)
        {
            if (instanceIdToGameObject.ContainsKey(instanceId))
                instanceIdToGameObject.Remove(instanceId);

            if (gameObjectToId.ContainsKey(gameObject))
                gameObjectToId.Remove(gameObject);
        }

        public bool TryGetInstanceId(GameObject entity, out int id, out int originalClientId)
        {
            if (gameObjectToId.ContainsKey(entity))
            {
                id = gameObjectToId[entity];
                originalClientId = instanceIdToInstanceData[id].originalClientID;
                return true;
            }

            id = -1;
            originalClientId = -1;
            return false;
        }

        public bool TryGetGameObject(int id, out GameObject entity)
        {
            if (instanceIdToGameObject.ContainsKey(id))
            {
                entity = instanceIdToGameObject[id];
                return true;
            }

            entity = null;
            return false;
        }

        public bool TryGetOriginalId(int id, out int originalId)
        {
            if (instanceIdToInstanceData.ContainsKey(id))
            {
                originalId = instanceIdToInstanceData[id].originalClientID;
                return true;
            }

            originalId = -1;
            return false;
        }

        private List<GameObject> GetAllGameObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            foreach (var instance in instanceIdToGameObject.Values)
            {
                gameObjects.Add(instance);
            }

            return gameObjects;
        }
    }
}