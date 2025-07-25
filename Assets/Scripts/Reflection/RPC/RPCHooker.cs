using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using HarmonyLib;
using Network;
using Network_dll.Messages.ClientMessages;
using Network_dll.Messages.Data;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;
using Random = System.Random;

namespace Reflection.RPC
{
    public class RPCHooker<ModelType> where ModelType : class, IReflectiveModel
    {
        public static RPCHooker<ModelType> Instance;
        private object _model;
        private Harmony _harmony;
        private Node _methodsTree;
        private ReflectiveClient<ModelType> _network;
        private ReflectiveAuthoritativeServer<ModelType> _server;

        private static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        #region Constructors

        public RPCHooker(ref ModelType model, ReflectiveAuthoritativeServer<ModelType> server)
        {
            if (Instance == null)
                Instance = this;

            _model = model;
            _server = server;
            _harmony = new Harmony("RPC Hooks");
            _methodsTree = new Node();
        }

        public RPCHooker(ref ModelType model, ReflectiveClient<ModelType> networkClient)
        {
            if (Instance == null)
                Instance = this;

            _model = model;
            _network = networkClient;
            _harmony = new Harmony("RPC Hooks");
            _methodsTree = new Node();
        }

        #endregion

        #region Harmony

        public void Hook()
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            GetMethods(_methodsTree, _model, methods);
            Debug.Log($"Methods Count: {methods.Count}");

            foreach (MethodInfo method in methods)
            {
                Debug.Log($"method: {method.Name}");
                Hook(method);
            }
        }

        public void AddHook(int[] route, object obj)
        {
            Node target = _methodsTree;
            for (int i = 0; i < route.Length - 1; i++)
            {
                target = target[route[i]];
            }

            List<MethodInfo> methods = new List<MethodInfo>();
            PopulateMethods(target, obj, methods);
            foreach (MethodInfo method in methods)
            {
                Debug.Log($"Hooked Method {method.Name}");
                Hook(method);
            }
        }

        private void Hook(MethodInfo method)
        {
            HarmonyMethod patch = new HarmonyMethod(typeof(RPCHooker<ModelType>).GetMethod(nameof(SendRPCMessage)));
            _harmony.Patch(method, postfix: patch);
        }

        private void Unhook(MethodInfo method)
        {
            _harmony.Unpatch(method, HarmonyPatchType.Postfix, _harmony.Id);
        }

        public static void SendRPCMessage(MethodBase __originalMethod)
        {
            RpcData data;
            int[] route = Instance.FindMethod(__originalMethod.Name);
            data.routeLength = route.Length;
            data.route = route;
            Node node = GetNode(Instance._methodsTree, route);
            RPCMessage message = new RPCMessage(data, node.attributes);

            if (Instance._network != null)
            {
                Debug.Log($"Rpc messageID: {Instance._network.Id}");
                message.clientId = Instance._network.Id;
                Instance._network?.SendToServer(message.Serialize());
            }

            if (Instance._server != null)
            {
                message.clientId = -1;
                Instance._server?.Broadcast(message.Serialize());
            }
        }

        #endregion

        #region TreeManagement

        private void GetMethods(Node rootNode, object obj, List<MethodInfo> methods)
        {
            PopulateMethods(rootNode, obj, methods);
            PopulateFields(rootNode, obj, methods);
        }

        private static void PopulateFields(Node rootNode, object obj, List<MethodInfo> methods)
        {
            if (obj == null)
                return;

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags))
            {
                Node child = new Node(rootNode);

                if (ReflectionUtilities.IsCollection(field))
                    PopulateCollection(child, field.GetValue(obj), methods);
                else if (field.FieldType.IsClass && field.FieldType != typeof(string) && field.FieldType != typeof(Random))
                {
                    PopulateMethods(child, field.GetValue(obj), methods);
                    PopulateFields(child, field.GetValue(obj), methods);
                    if (!ContainsRPCMethod(child))
                        rootNode.RemoveChild(child);
                }
            }
        }

        private static void PopulateMethods(Node rootNode, object obj, List<MethodInfo> methods)
        {
            if (obj == null)
                return;

            foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags))
            {
                Node methodNode = new Node(rootNode);
                RPCAttribute rpcAttribute = (RPCAttribute)method.GetCustomAttribute(typeof(RPCAttribute), false);
                if (rpcAttribute != null)
                {
                    methodNode.ShouldSync = true;
                    methodNode.attributes = rpcAttribute.attributes;
                    methods.Add(method);
                }
            }
        }

        public static void PopulateCollection(Node parent, object obj, List<MethodInfo> methods)
        {
            if (obj == null)
                return;

            foreach (object item in obj as ICollection)
            {
                if (item == null)
                    continue;

                if (ReflectionUtilities.IsCollection(item))
                {
                    Node subChild = new Node(parent);
                    PopulateCollection(subChild, item, methods);
                }
                else if (PrimitiveUtils.GetObjectType(item) == PrimitiveType.NonPrimitive && item.GetType() != typeof(string) && item.GetType() != typeof(Random))
                {
                    Node subChild = new Node(parent);

                    PopulateMethods(subChild, item, methods);
                    PopulateFields(subChild, item, methods);

                    if (!subChild.ContainsSyncedNodes)
                        subChild.RemoveAllChildren();
                }
            }
        }

        private static bool ContainsRPCMethod(Node rootNode)
        {
            foreach (Node child in rootNode.Children)
            {
                if (child.ShouldSync)
                    return true;

                return ContainsRPCMethod(child);
            }

            return false;
        }

        private MethodInfo GetMethodAt(object obj, int[] route, int startIndex = 0)
        {
            if (startIndex < route.Length - 1)
            {
                FieldInfo info = obj.GetType().GetFields(BindingFlags)[route[startIndex]];
                return GetMethodAt(info.GetValue(obj), route, startIndex + 1);
            }

            return obj.GetType().GetMethods(BindingFlags)[route[startIndex]];
        }

        private MethodInfo GetMethodAt(object obj, int[] route, out object methodHolder, int startIndex = 0)
        {
            if (startIndex < route.Length - 1)
            {
                FieldInfo info = obj.GetType().GetFields(BindingFlags)[route[startIndex]];
                return GetMethodAt(info.GetValue(obj), route, out methodHolder, startIndex + 1);
            }

            methodHolder = obj;
            return obj.GetType().GetMethods(BindingFlags)[route[startIndex]];
        }

        private int[] FindMethod(string name, Node root = null)
        {
            if (root == null)
                root = _methodsTree;

            foreach (Node child in root.Children)
            {
                if (child.ShouldSync)
                {
                    string childName = GetMethodAt(_model, GetMethodNodeRoute(child)).Name;
                    if (childName == name)
                        return GetMethodNodeRoute(child);
                }
                else if (child.Children.Length > 0)
                    return FindMethod(name, child);
            }

            return null;
        }

        private int[] GetMethodNodeRoute(Node node, List<int> indices = null)
        {
            indices ??= new List<int>();

            Node parent = node.Parent;
            if (parent != null)
            {
                GetMethodNodeRoute(parent, indices);
                int path = parent.GetChildIndex(node) - GetMethodsCount(parent);
                if (node.ShouldSync)
                    path++;

                indices.Add(path);
            }

            return indices.ToArray();
        }

        private int GetMethodsCount(Node node)
        {
            int qtyOfMethods = 0;
            foreach (Node child in node.Children)
                if (child.ShouldSync)
                    qtyOfMethods++;

            return qtyOfMethods;
        }

        private static Node GetNode(Node root, int[] route, int startIndex = 0)
        {
            if (startIndex < route.Length - 1)
                return GetNode(root[route[startIndex]], route, startIndex + 1);

            return root[route[startIndex]];
        }

        #endregion

        public void ReceiveRPCMessage(RpcData messageData)
        {
            MethodInfo method = GetMethodAt(_model, messageData.route, out object objectHolder);
            Unhook(method);
            method.Invoke(objectHolder, new object[] { });
            Hook(method);
        }
    }
}