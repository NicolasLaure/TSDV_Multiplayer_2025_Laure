using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Reflection
{
    public class Reflection : MonoBehaviour
    {
        private TestModel _testModel = new TestModel();
        private Node root;

        [ContextMenu("Reflect")]
        public void ReflectModel()
        {
            root = PopulateTree(_testModel);
            Debug.Log($"Root children count: {root.Count}");
            Debug.Log($"Value At [2][2]: {root[2][2].nodeObject}");
        }

        private Node PopulateTree(object obj)
        {
            Node rootNode = new Node(obj);

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Node childNode = new Node(field.GetValue(obj), rootNode);
                if (field.FieldType != typeof(string) &&
                    (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType)))
                {
                    Debug.Log($"Field{RouteString(childNode.GetRoute())} with Name: {field.Name} IS A COLLECTION");
                    foreach (object item in field.GetValue(obj) as ICollection)
                    {
                        childNode.AddChild(PopulateTree(item));
                    }
                }
                else if (!field.FieldType.IsPrimitive)
                {
                    Debug.Log($"Field{RouteString(childNode.GetRoute())}: with name: {field.Name} Is not primitive");
                    childNode.AddChild(PopulateTree(field.GetValue(obj)));
                }

                Debug.Log($"Field{RouteString(childNode.GetRoute())} with name: {field.Name}, and Value: {field.GetValue(obj)}");
            }

            return rootNode;
        }

        private void Reflect(object obj)
        {
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                                BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType != typeof(string) &&
                    (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType)))
                {
                    Debug.Log($"Field Name: {field.Name} IS COLLECTION");
                    foreach (object item in field.GetValue(obj) as ICollection)
                    {
                        Reflect(item);
                    }
                }
                else if (!field.FieldType.IsPrimitive)
                {
                    Debug.Log($"Field: {field.Name} Is not primitive");
                    Reflect(field.GetValue(obj));
                }

                Debug.Log("Field Name: " + field.Name + ", Value: " + field.GetValue(obj));
            }
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
    }
}