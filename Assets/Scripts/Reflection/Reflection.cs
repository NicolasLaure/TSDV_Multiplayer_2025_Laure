using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ReflectionTest;
using UnityEngine;

namespace Reflection
{
    public class Reflection : MonoBehaviour
    {
        private TestModel _testModel = new TestModel();
        private Node root;
        private List<int[]> dirtyRoutes = new List<int[]>();

        private void Start()
        {
            root = PopulateTree(_testModel);
            Debug.Log($"Root children count: {root.Count}");
        }

        private void Update()
        {
            dirtyRoutes.Clear();
            DirtyRegistry.GetDirtyNodes(root, ref dirtyRoutes);
            foreach (int[] route in dirtyRoutes)
            {
                Debug.Log($"DIRTY: {RouteString(route)}");
            }
        }

        private Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node(obj);

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Node childNode = new Node(field.GetValue(obj));
                if (ShouldSync(field) || root.ShouldSync)
                    childNode.ShouldSync = true;

                Debug.Log($"Node{RouteString(childNode.GetRoute())} = {childNode.nodeObject}");
                if (field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType)))
                {
                    foreach (object item in field.GetValue(obj) as ICollection)
                    {
                        PopulateTree(item, childNode);
                    }
                }
                else if (!field.FieldType.IsPrimitive)
                {
                    PopulateTree(field.GetValue(obj), childNode);
                }

                if (childNode.ShouldSync || childNode.ContainsSyncedNodes)
                    childNode.SetParent(root);
            }

            return root;
        }

        private bool ShouldSync(FieldInfo info)
        {
            return info.GetCustomAttribute(typeof(Sync), false) != null;
        }

        private object GetDataAt(int[] route)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            return target.nodeObject;
        }

        private void SetDataAt(int[] route, object value, bool isRemoteData)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            target.UpdateValue(value, isRemoteData);
        }

        private string RouteString(int[] route)
        {
            string routeString = "";
            for (int i = 0; i < route.Length; i++)
            {
                routeString += $"[{route[i]}]";
            }

            return routeString;
        }

        [ContextMenu("Test")]
        private void Test()
        {
            int[] route = { 0, 1 };
            SetDataAt(route, 12, false);
            int[] routeB = { 1, 0, 2 };
            SetDataAt(routeB, 123, true);
        }
    }
}