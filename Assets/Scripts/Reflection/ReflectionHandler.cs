using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Network_dll.Messages.Data;
using Network;
using Network.Enums;
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
        private NetworkClient _networkClient;
        public RPCHooker<ModelType> rpcHooker;
        public Node Root => root;
        private DirtyRegistry<ModelType> _dirtyRegistry = new DirtyRegistry<ModelType>();

        private BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public ReflectionHandler(ref ModelType model)
        {
            _model = model;
            root = ReflectionUtilities.PopulateTree(_model);
            rpcHooker = new RPCHooker<ModelType>(ref model);
            rpcHooker.Hook();
        }

        public ReflectionHandler(ref ModelType model, NetworkClient networkClient)
        {
            _model = model;
            _networkClient = networkClient;
            root = ReflectionUtilities.PopulateTree(_model);
            rpcHooker = new RPCHooker<ModelType>(ref model, _networkClient);
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
            data.type = PrimitiveUtils.GetObjectType(GetObjectAt(route, _model));
            data.routeLength = route.Length;
            data.route = route;
            data.obj = GetObjectAt(route, _model);
            return new PrimitiveMessage(data, target.attributes);
        }

        public void SetData(int[] route, object value)
        {
            SetDataAt(route, value, _model);
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            target.lastHash = value.GetHashCode();
        }

        public object SetDataAt(int[] route, object value, object obj, int startIndex = 0)
        {
            if (startIndex >= route.Length)
                return value;

            FieldInfo info = obj.GetType().GetFields(_bindingFlags)[route[startIndex]];
            if (info.FieldType != typeof(string) && (info.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(info.FieldType)))
            {
                int index = 0;
                foreach (object item in info.GetValue(obj) as ICollection)
                {
                    if (index == route[startIndex + 1])
                    {
                        FieldInfo itemInfo = item.GetType().GetFields(_bindingFlags)[route[startIndex + 2]];
                        itemInfo.SetValue(item, SetDataAt(route, value, itemInfo.GetValue(item), startIndex + 3));
                        return item;
                    }

                    index++;
                }

                return null;
            }

            obj.GetType().GetFields(_bindingFlags)[route[startIndex]].SetValue(obj, SetDataAt(route, value, info.GetValue(obj), startIndex + 1));
            return obj;
        }

        private object GetObjectAt(int[] route, object obj, int startIndex = 0)
        {
            if (startIndex >= route.Length)
                return obj;

            FieldInfo info = obj.GetType().GetFields(_bindingFlags)[route[startIndex]];
            if (info.FieldType != typeof(string) && (info.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(info.FieldType)))
            {
                int index = 0;
                foreach (object item in info.GetValue(obj) as ICollection)
                {
                    if (index == route[startIndex + 1])
                        return GetObjectAt(route, item, startIndex + 2);

                    index++;
                }

                return null;
            }

            return GetObjectAt(route, info.GetValue(obj), startIndex + 1);
        }

        public object GetDataAt(int[] route)
        {
            return GetObjectAt(route, _model);
        }


        private void UpdateHashes(Node rootNode)
        {
            if (rootNode.ShouldSync)
                rootNode.currentHash = GetObjectAt(rootNode.GetRoute(), _model).GetHashCode();

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
    }
}