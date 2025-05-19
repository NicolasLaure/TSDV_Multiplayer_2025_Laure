using System.Collections.Generic;
using Network.Messages;
using UnityEngine;

namespace Network.Factory
{
    public class AuthServerFactory : NetworkFactory
    {
        private readonly Dictionary<int, InstanceData> heldInstanceIdToData = new Dictionary<int, InstanceData>();
        private readonly Dictionary<int, GameObject> instanceIdToGameObject = new Dictionary<int, GameObject>();
        private readonly Dictionary<GameObject, int> gameObjectToId = new Dictionary<GameObject, int>();

        private HashHandler _prefabsData;
        private ColorHandler _colorHandler;

        public AuthServerFactory(HashHandler prefabData, ColorHandler colorHandler)
        {
            _prefabsData = prefabData;
            _colorHandler = colorHandler;
        }

        public GameObject Instantiate(InstanceData instanceData)
        {
            if (!_prefabsData.hashToPrefab.ContainsKey(instanceData.prefabHash))
            {
                Debug.Log("Couldn't Find prefab");
                return null;
            }

            GameObject prefab = _prefabsData.hashToPrefab[instanceData.prefabHash];
            Matrix4x4 trs = ByteFormat.Get4X4FromBytes(instanceData.trs, 0);
            GameObject instance = GameObject.Instantiate(prefab, trs.GetPosition(), trs.rotation);
            if (instance.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
            {
                meshRenderer.material = _colorHandler.GetFromColor(instanceData.color);
            }

            instance.GetComponent<IInstantiable>().SetScripts();

            SaveGameObject(instanceData, instance);
            return instance;
        }

        void BroadcastInstantiation(InstanceData instanceData, AuthoritativeServer server)
        {
            SaveInstance(ref instanceData);
            heldInstanceIdToData.Add(instanceData.instanceID, instanceData);
            server.Broadcast(new InstantiateRequest(instanceData).Serialize());
        }

        public void BroadcastDeInstantiation(int clientId, int instanceId, AuthoritativeServer server)
        {
            if (instanceIdToInstanceData.ContainsKey(instanceId) && instanceIdToInstanceData[instanceId].originalClientID == clientId)
            {
                server.Broadcast(new DeInstantiateRequest(instanceId).Serialize());
                RemoveInstance(instanceId);
            }
        }

        public void BroadcastDeInstantiation(int instanceId, AuthoritativeServer server)
        {
            server.Broadcast(new DeInstantiateRequest(instanceId).Serialize());
            RemoveInstance(instanceId);
        }

        public InstantiateAll GetObjectsToInstantiate()
        {
            List<InstanceData> instanceDatas = new List<InstanceData>();
            foreach (var instanceData in instanceIdToInstanceData.Values)
            {
                instanceDatas.Add(instanceData);
            }

            return new InstantiateAll(instanceDatas);
        }

        public void CheckIntegrity(List<InstanceData> instanceDatas, int instanceId, AuthoritativeServer server)
        {
            bool isIntegral = true;
            for (int i = 0; isIntegral && i < instanceDatas.Count; i++)
            {
                for (int j = i; j < instanceDatas.Count; j++)
                {
                    if (instanceDatas[i] == instanceDatas[j]) continue;

                    Debug.LogError($"Instances Weren't similar, First Id was {instanceDatas[i].instanceID} Second Id was {instanceDatas[j].instanceID}");
                    isIntegral = false;
                    break;
                }
            }

            if (isIntegral && heldInstanceIdToData.ContainsKey(instanceId))
            {
                Debug.Log("Instance Was Integral");
                heldInstanceIdToData.Remove(instanceId);
                return;
            }

            BroadcastDeInstantiation(instanceId, server);
            BroadcastInstantiation(heldInstanceIdToData[instanceId], server);
            heldInstanceIdToData.Remove(instanceId);
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