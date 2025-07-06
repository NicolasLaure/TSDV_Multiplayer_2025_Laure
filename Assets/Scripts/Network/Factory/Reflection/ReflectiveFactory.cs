using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cubes;
using MidTerm2;
using MidTerm2.Model;
using MidTerm2.View;
using Network.Factory.Reflection;
using Network.Messages;
using Reflection;
using Unity.Mathematics;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Network.Factory
{
    public struct ObjectModel
    {
        public int[] route;
        public GameObject view;
    }

    public class ReflectiveFactory<ModelType> : NetworkFactory where ModelType : class, IReflectiveModel
    {
        private readonly Dictionary<int, ObjectModel> instanceIdToObject = new Dictionary<int, ObjectModel>();
        private readonly Dictionary<ObjectModel, int> ObjectToId = new Dictionary<ObjectModel, int>();
        public readonly ReflectiveHashHandler typeHashes;
        public HashHandler prefabHashes;
        private readonly ColorHandler _colorHandler;

        private ReflectionHandler<ModelType> _reflection;

        public ReflectiveFactory(ReflectionHandler<ModelType> reflection, List<Type> instantiableTypes, ColorHandler colorHandler, HashHandler prefabHash)
        {
            _reflection = reflection;
            typeHashes = new ReflectiveHashHandler(instantiableTypes);
            _colorHandler = colorHandler;
            prefabHashes = prefabHash;
        }

        public InstanceData Instantiate(InstanceData instanceData)
        {
            if (!typeHashes.hashToType.ContainsKey(instanceData.prefabHash) || !prefabHashes.hashToPrefab.ContainsKey(instanceData.prefabHash))
            {
                Debug.Log("Couldn't Find type");
                return new InstanceData();
            }

            object instance = Activator.CreateInstance(typeHashes.hashToType[instanceData.prefabHash]);
            GameObject prefab = prefabHashes.hashToPrefab[instanceData.prefabHash];
            Matrix4x4 trs = ByteFormat.Get4X4FromBytes(instanceData.trs, 0);
            GameObject instanceGO = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            if (instanceGO.TryGetComponent<SpriteRenderer>(out SpriteRenderer meshRenderer))
                meshRenderer.color = _colorHandler.GetFromColor(instanceData.color).color;

            Vector2 pos = new Vector2(trs.GetPosition().x, trs.GetPosition().y);
            if (instanceData.originalClientID == CastlesClient.Instance.clientId)
            {
                if (instance.GetType() == typeof(Castle) || instance.GetType() == typeof(Warrior))
                    ((CastlesModel)_reflection._model).SetTileObject((TileObject)instance, pos);
            }

            SaveObject(instanceData, instance, instanceGO);

            if (instanceGO.TryGetComponent(out TileObjectView viewObject))
                CastlesView.Instance.SetTileObjectPosition(instanceGO, trs.GetPosition());

            return instanceData;
        }

        public void DeInstantiate(int instanceId)
        {
            GameObject gameObjectToDestroy = instanceIdToObject[instanceId].view;
            RemoveObject(instanceId);
            GameObject.Destroy(gameObjectToDestroy);
        }

        public void InstantiateMultiple(InstantiateAll objectsToInstantiate)
        {
            for (int i = 0; i < objectsToInstantiate.count; i++)
            {
                Instantiate(objectsToInstantiate.instancesData[i]);
            }
        }

        public void DeInstantiateAll()
        {
            List<ObjectModel> allGameObjects = GetAllGameObjects();
            for (int i = 0; i < allGameObjects.Count; i++)
            {
                RemoveObject(ObjectToId[allGameObjects[i]]);
                GameObject.Destroy(allGameObjects[i].view);
            }
        }

        private void SaveObject(InstanceData instanceData, object obj, GameObject gameObject)
        {
            instanceIdToInstanceData[instanceData.instanceID] = instanceData;
            ObjectModel newObject;
            newObject.route = instanceData.route;
            newObject.view = gameObject;
            _reflection.SetData(instanceData.route, obj);

            instanceIdToObject[instanceData.instanceID] = newObject;
            ObjectToId[newObject] = instanceData.instanceID;
        }

        private void RemoveObject(int instanceId)
        {
            if (instanceIdToObject.ContainsKey(instanceId))
            {
                instanceIdToObject.Remove(instanceId);

                _reflection.SetData(instanceIdToObject[instanceId].route, null);
                if (ObjectToId.ContainsKey(instanceIdToObject[instanceId]))
                    ObjectToId.Remove(instanceIdToObject[instanceId]);
            }
        }

        public bool TryGetInstanceId(ObjectModel entity, out int id, out int originalClientId)
        {
            if (ObjectToId.ContainsKey(entity))
            {
                id = ObjectToId[entity];
                originalClientId = instanceIdToInstanceData[id].originalClientID;
                return true;
            }

            id = -1;
            originalClientId = -1;
            return false;
        }

        public bool TryGetGameObject(int id, out GameObject entity)
        {
            if (instanceIdToObject.ContainsKey(id))
            {
                entity = instanceIdToObject[id].view;
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

        private List<ObjectModel> GetAllGameObjects()
        {
            List<ObjectModel> gameObjects = new List<ObjectModel>();
            foreach (var instance in instanceIdToObject.Values)
            {
                gameObjects.Add(instance);
            }

            return gameObjects;
        }
    }
}