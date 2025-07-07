using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network;
using Network_dll.Messages.Data;
using Network.Messages;
using Reflection.RPC;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    public class ReflectionHandler<ModelType> where ModelType : class, IReflectiveModel
    {
        private Node root;
        public object _model;
        private ReflectiveClient<ModelType> _networkClient;
        public RPCHooker<ModelType> rpcHooker;
        public Node Root => root;
        private DirtyRegistry<ModelType> _dirtyRegistry = new DirtyRegistry<ModelType>();

        public ReflectionHandler(ref ModelType model)
        {
            _model = model;
            rpcHooker = new RPCHooker<ModelType>(ref model);
            Initialize();
        }

        public ReflectionHandler(ref ModelType model, ReflectiveClient<ModelType> networkClient)
        {
            _model = model;
            _networkClient = networkClient;
            rpcHooker = new RPCHooker<ModelType>(ref model, _networkClient);
            Initialize();
        }

        private void Initialize()
        {
            root = ReflectionUtilities.PopulateTree(_model);
            rpcHooker.Hook();
        }

        public void Update()
        {
            UpdateHashes(root);
            _dirtyRegistry.Update(root);
            SendDirtyValues();
        }

        public PrimitiveMessage GetMessage(int[] route)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            PrimitiveData data;
            data.type = PrimitiveUtils.GetObjectType(ReflectionUtilities.GetObjectAt(route, _model));
            data.routeLength = route.Length;
            data.route = route;
            data.obj = ReflectionUtilities.GetObjectAt(route, _model);
            return new PrimitiveMessage(data, target.attributes);
        }

        public void SetData<T>(int[] route, T value)
        {
            SetDataAt(route, value, _model);
            Node target = root;

            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            target.lastHash = value.GetHashCode();
        }

        public object GetDataAt(int[] route)
        {
            return ReflectionUtilities.GetObjectAt(route, _model);
        }

        public Type GetObjectType(int[] route)
        {
            object obj = ReflectionUtilities.GetObjectAt(route, _model);
            return obj.GetType();
        }

        private void UpdateHashes(Node rootNode)
        {
            if (rootNode.ShouldSync)
                rootNode.currentHash = ReflectionUtilities.GetObjectAt(rootNode.GetRoute(), _model).GetHashCode();

            if (rootNode.ContainsSyncedNodes)
                foreach (Node child in rootNode.Children)
                {
                    UpdateHashes(child);
                }
        }

        private void SendDirtyValues()
        {
            if (_networkClient == null)
                return;

            foreach (int[] route in _dirtyRegistry.DirtyRoutes)
            {
                PrimitiveMessage message = GetMessage(route);
                if (message.data.type != PrimitiveType.NonPrimitive)
                {
                    Debug.Log($"Sent PrimitiveData: {message.data.obj}");
                    _networkClient.SendToServer(message.Serialize());
                }
            }
        }

        public void ReceiveValues(PrimitiveData data)
        {
            Debug.Log($"Received primitive: {data.obj}");
            SetData(data.route, data.obj);
        }

        public object SetDataAt<T>(int[] route, T value, object obj, int startIndex = 0)
        {
            if (startIndex >= route.Length)
                return value;

            FieldInfo info = obj.GetType().GetFields(ReflectionUtilities.bindingFlags)[route[startIndex]];
            if (startIndex == route.Length - 1)
            {
                if (info.IsCollection())
                {
                    Debug.Log("SET Collection");
                    info.SetValue(obj, SetCollectionData(info.GetValue(obj), route, value, startIndex + 1));
                    AddNode(route, value);

                    return info.GetValue(obj);
                }

                return value;
            }

            if (info.IsCollection())
            {
                info.SetValue(obj, SetCollectionData(info.GetValue(obj), route, value, startIndex + 1));
            }
            else
                info.SetValue(obj, SetDataAt(route, value, info.GetValue(obj), startIndex + 1));

            return info.GetValue(obj);
        }

        public object SetCollectionData<T>(object obj, int[] route, T value, int startIndex = 0)
        {
            object[] objectRef;
            if (obj is not ICollection<T> collection)
            {
                Debug.Log($"T: {typeof(T)}: Collection Type: {obj.GetType().GetCollectionType()}");
                return null;
            }

            objectRef = new object[collection.Count + 1];

            for (int i = 0; i < objectRef.Length; i++)
            {
                if (i < collection.Count)
                {
                    objectRef[i] = (obj as ICollection).Cast<object>().ElementAt(i);
                }
                else
                {
                    objectRef[i] = Activator.CreateInstance(value.GetType());
                    objectRef[i] = value;
                }
            }

            Debug.Log($"New list count: {objectRef.Length}");

            // if (startIndex >= route.Length - 1)
            // {
            //     return list;
            // }

            // if (list[route[startIndex]] != null && list[route[startIndex]].IsCollection())
            //     return SetCollectionData(list[route[startIndex]], route, value, startIndex + 1);
            return Activator.CreateInstance(obj.GetType(), TransaltorICollection<T>(objectRef) as ICollection);
        }

        private void AddNode(int[] route, object value)
        {
            Node target = root;
            for (int i = 0; i < route.Length - 1; i++)
            {
                target = target[route[i]];
            }

            new Node(target);
            Debug.Log($"Add List Member");
            //rpcHooker.AddHook(route, value);
        }

        private object TransaltorICollection<T>(object[] objs)
        {
            List<T> listToTranslate = new List<T>();
            foreach (object elementsOfObjets in objs)
            {
                listToTranslate.Add((T)elementsOfObjets);
            }

            return listToTranslate;
        }
    }
}