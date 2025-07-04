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
            root = PopulateTree(_model);
            rpcHooker = new RPCHooker<ModelType>(ref model);
            rpcHooker.Hook();
        }

        public ReflectionHandler(ref ModelType model, NetworkClient networkClient)
        {
            _model = model;
            _networkClient = networkClient;
            root = PopulateTree(_model);
            rpcHooker = new RPCHooker<ModelType>(ref model, _networkClient);
            rpcHooker.Hook();
        }

        public void Update()
        {
            UpdateHashes(root);
            _dirtyRegistry.Update(root);
            SendDirtyValues();
        }

        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node();

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Node childNode = new Node();
                if (ShouldSync(field))
                {
                    childNode.ShouldSync = true;
                    childNode.attributes = GetAttribs(field);
                }
                else if (root.ShouldSync)
                {
                    childNode.ShouldSync = true;
                    childNode.attributes = root.attributes;
                }

                if (childNode.ShouldSync)
                    childNode.SetParent(root);

                if (field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType)))
                {
                    foreach (object item in field.GetValue(obj) as ICollection)
                    {
                        if (PrimitiveUtils.GetObjectType(item) == PrimitiveType.NonPrimitive)
                        {
                            Node subChild = new Node(childNode);

                            PopulateTree(item, subChild);

                            if (!subChild.ContainsSyncedNodes)
                                subChild.RemoveAllChildren();
                        }
                    }
                }
                else if (!field.FieldType.IsPrimitive)
                {
                    PopulateTree(field.GetValue(obj), childNode);
                }

                if (!childNode.ContainsSyncedNodes)
                    childNode.RemoveAllChildren();

                childNode.SetParent(root);
            }

            return root;
        }

        private static bool ShouldSync(FieldInfo info)
        {
            return info.GetCustomAttribute(typeof(Sync), false) != null;
        }

        private static Attributes GetAttribs(FieldInfo info)
        {
            return ((Sync)info.GetCustomAttribute(typeof(Sync), false)).attribs;
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