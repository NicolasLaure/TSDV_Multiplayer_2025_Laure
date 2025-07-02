using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor.Rendering;
using UnityEngine;
using Utils;

namespace Reflection.RPC
{
    public class RPCHooker<ModelType> where ModelType : class, IReflectiveModel
    {
        public static RPCHooker<ModelType> Instance;
        private object _model;
        private Harmony _harmony;
        private Node methodsTree;

        private static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public RPCHooker(ref ModelType model)
        {
            if (Instance == null)
                Instance = this;

            _model = model;
            _harmony = new Harmony("RPC Hooks");
            methodsTree = new Node();
        }

        public void Hook()
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            GetMethods(methodsTree, _model, methods);
            Debug.Log($"Methods Count: {methods.Count}");

            foreach (MethodInfo method in methods)
            {
                HarmonyMethod patch = new HarmonyMethod(typeof(RPCHooker<ModelType>).GetMethod(nameof(SendRPCMessage)));
                _harmony.Patch(method, postfix: patch);
            }
        }

        private void GetMethods(Node rootNode, object obj, List<MethodInfo> methods)
        {
            foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags))
            {
                Node methodNode = new Node(rootNode);
                if (method.GetCustomAttribute(typeof(RPCAttribute), false) != null)
                {
                    methodNode.ShouldSync = true;
                    Debug.Log($"Adding Method with Route: {Route.RouteString(GetMethodNodeRoute(methodNode))}");
                    methods.Add(method);
                }
            }

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags))
            {
                Node child = new Node(rootNode);

                if (!field.FieldType.IsPrimitive)
                {
                    GetMethods(child, field.GetValue(obj), methods);
                    if (!ContainsRPCMethod(child))
                        rootNode.RemoveChild(child);
                }
            }
        }

        private bool ContainsRPCMethod(Node rootNode)
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

        private int[] FindMethod(string name, Node root = null)
        {
            if (root == null)
                root = methodsTree;

            foreach (Node child in root.Children)
            {
                if (child.ShouldSync)
                {
                    string childName = GetMethodAt(_model, GetMethodNodeRoute(child)).Name;
                    Debug.Log($"ChildName: {childName}");
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

        public static void SendRPCMessage(MethodBase __originalMethod)
        {
            Debug.Log($"Send Method with Route {Route.RouteString(Instance.FindMethod(__originalMethod.Name))} To server");
        }
    }
}