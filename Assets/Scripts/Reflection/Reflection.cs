using System.Collections;
using System.Reflection;
using ReflectionTest;
using UnityEngine;

namespace Reflection
{
    public class Reflection : MonoBehaviour
    {
        private TestModel _testModel = new TestModel();
        private Node root;

        private void Start()
        {
            ReflectModel();
        }

        public void ReflectModel()
        {
            root = PopulateTree(_testModel);
            Debug.Log($"Root children count: {root.Count}");
            // Debug.Log($"Value At [2][2]: {root[2][2].nodeObject}");
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

        private void SetDataAt(int[] route, object value)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            target.nodeObject = value;
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
            int[] a = { 2, 1 };
            int[] b = { 3, 1 };

            Debug.Log($"Data at[2][1] = {GetDataAt(a)}");

            Debug.Log($"Data at[3][1] = {GetDataAt(b)}");
            SetDataAt(b, 321);
            Debug.Log($"Data at[3][1] = {GetDataAt(b)}");
        }
    }
}