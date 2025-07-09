using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network;
using Network_dll.Messages.Data;
using Network.Messages;
using NUnit.Framework.Internal.Filters;
using Reflection.RPC;
using UnityEditor.UIElements;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    [Serializable]
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

        public void SetData<T>(int[] route, object value)
        {
            SetDataAt<T>(route, value, _model);

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

        public void ReceiveValues<T>(PrimitiveData data)
        {
            Debug.Log($"Received primitive: {data.obj}: Route: {Route.RouteString(data.route)}");
            SetData<T>(data.route, data.obj);
        }

        public object SetDataAt<T>(int[] route, object value, object obj, int startIndex = 0)
        {
            if (startIndex >= route.Length)
                return value;

            FieldInfo info = obj.GetType().GetFields(ReflectionUtilities.bindingFlags)[route[startIndex]];

            if (info.IsCollection())
            {
                Debug.Log("Set CollectionData");
                info.SetValue(obj, SetCollectionsData<T>(info.GetValue(obj), route, value, startIndex + 1));
            }
            else
                info.SetValue(obj, SetDataAt<T>(route, value, info.GetValue(obj), startIndex + 1));

            return obj;
        }

        public object SetCollectionData<T>(object obj, int[] route, object value, int index = 0)
        {
            if (obj is not ICollection collection)
                return null;

            object[] objectRef = new object[collection.Count];

            if (index >= collection.Count)
                objectRef = new object[index + 1];

            for (int i = 0; i < objectRef.Length; i++)
            {
                if (i < collection.Count)
                {
                    if (i == index)
                        objectRef[i] = value;
                    else
                        objectRef[i] = (obj as ICollection).Cast<object>().ElementAt(i);
                }
                else
                {
                    objectRef[i] = Activator.CreateInstance(value.GetType());
                    objectRef[i] = value;
                    AddNode(route, value);
                }
            }

            return Activator.CreateInstance(obj.GetType(), TranslatorICollection<T>(objectRef) as ICollection);
        }

        public object SetCollectionsData<T>(object obj, int[] route, object value, int startIndex = 0)
        {
            if (!obj.IsCollection())
                return obj;

            if (startIndex >= route.Length - 1)
                return SetCollectionData<T>(obj, route, value, route[^1]);

            int index = 0;
            foreach (object item in obj as ICollection)
            {
                if (index == route[startIndex])
                {
                    if (item.IsCollection())
                        return SetCollectionsData<T>(item, route, value, startIndex + 1);

                    if (item.GetType().IsClass)
                    {
                        object valueSetted = SetDataAt<T>(route, value, item, startIndex + 1);
                        return typeof(ReflectionHandler<ModelType>).GetMethod(nameof(SetCollectionData),
                        BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(obj.GetType()).Invoke(
                        this, new[] { obj, route, valueSetted, route[startIndex] });
                    }

                    return SetCollectionData<T>(obj, route, value, route[startIndex]);
                }

                index++;
            }

            return null;
        }

        private void AddNode(int[] route, object value)
        {
            Node target = root;
            for (int i = 0; i < route.Length - 1; i++)
            {
                target = target[route[i]];
            }

            Node child = ReflectionUtilities.PopulateTree(value);
            child.SetParent(target);
        }

        private object TranslatorICollection<T>(object[] objs)
        {
            List<T> listToTranslate = new List<T>();
            Debug.Log($"Typeof T: {typeof(T)}");
            foreach (object elementsOfObjets in objs)
            {
                Debug.Log($"Typeof object: {elementsOfObjets.GetType()}");
                listToTranslate.Add((T)elementsOfObjets);
            }

            return listToTranslate;
        }
    }
}