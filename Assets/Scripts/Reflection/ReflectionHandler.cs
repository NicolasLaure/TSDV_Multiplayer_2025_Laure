using System;
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
            root = ReflectionUtilities.PopulateTree(_model);
            rpcHooker = new RPCHooker<ModelType>(ref model);
            rpcHooker.Hook();
        }

        public ReflectionHandler(ref ModelType model, ReflectiveClient<ModelType> networkClient)
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
            data.type = PrimitiveUtils.GetObjectType(ReflectionUtilities.GetObjectAt(route, _model));
            data.routeLength = route.Length;
            data.route = route;
            data.obj = ReflectionUtilities.GetObjectAt(route, _model);
            return new PrimitiveMessage(data, target.attributes);
        }

        public void SetData(int[] route, object value)
        {
            ReflectionUtilities.SetDataAt(route, value, _model);
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
    }
}