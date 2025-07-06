using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Network.Enums;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    public static class ReflectionUtilities
    {
        private static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node();
            if (obj == null)
                return root;

            foreach (FieldInfo field in obj.GetType().GetFields(_bindingFlags))
            {
                Debug.Log($"fieldName: {field.Name}");
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

                if (IsCollection(field) && field.GetValue(obj) != null)
                    PopulateCollection(childNode, field.GetValue(obj));
                else if (!field.FieldType.IsPrimitive)
                    PopulateTree(field.GetValue(obj), childNode);

                if (!childNode.ContainsSyncedNodes)
                    childNode.RemoveAllChildren();

                childNode.SetParent(root);
            }

            return root;
        }

        public static object GetObjectAt(int[] route, object obj, int startIndex = 0)
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

        public static bool TryGetRoute(object model, object obj, List<int> route)
        {
            int count = 0;
            foreach (FieldInfo field in model.GetType().GetFields(_bindingFlags))
            {
                if (ReferenceEquals(field.GetValue(model), obj))
                {
                    route.Insert(0, count);
                    return true;
                }

                if (!field.FieldType.IsPrimitive)
                {
                    if (TryGetRoute(field.GetValue(model), obj, route))
                    {
                        route.Insert(0, count);
                    }
                }

                count++;
            }

            return false;
        }

        public static object SetDataAt(int[] route, object value, object obj, int startIndex = 0)
        {
            if (startIndex >= route.Length)
                return value;

            FieldInfo info = obj.GetType().GetFields(_bindingFlags)[route[startIndex]];
            if (IsCollection(info))
                return SetCollectionData(info.GetValue(obj), route, value, startIndex + 1);

            obj.GetType().GetFields(_bindingFlags)[route[startIndex]].SetValue(obj, SetDataAt(route, value, info.GetValue(obj), startIndex + 1));
            return obj;
        }

        public static bool ShouldSync(FieldInfo info)
        {
            return info.GetCustomAttribute(typeof(Sync), false) != null;
        }

        public static Attributes GetAttribs(FieldInfo info)
        {
            return ((Sync)info.GetCustomAttribute(typeof(Sync), false)).attribs;
        }

        public static bool IsCollection(object obj)
        {
            return obj.GetType() != typeof(string) && (obj.GetType().IsArray || obj is ICollection);
        }

        public static bool IsCollection(FieldInfo field)
        {
            return field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType));
        }

        public static void PopulateCollection(Node parent, object obj)
        {
            if (obj == null)
                return;

            foreach (object item in obj as ICollection)
            {
                if (item == null)
                    continue;

                if (IsCollection(item))
                {
                    Node subChild = new Node(parent);
                    PopulateCollection(subChild, item);
                }
                else if (PrimitiveUtils.GetObjectType(item) == PrimitiveType.NonPrimitive)
                {
                    Node subChild = new Node(parent);

                    PopulateTree(item, subChild);

                    if (!subChild.ContainsSyncedNodes)
                        subChild.RemoveAllChildren();
                }
            }
        }

        public static object GetCollectionData(object obj, int[] route, int startIndex = 0)
        {
            if (!IsCollection(obj))
                return null;

            ICollection<object> collection = obj as ICollection<object>;
            List<object> list = new List<object>();
            using (IEnumerator<object> iterator = collection.GetEnumerator())
            {
                while (iterator.MoveNext())
                    list.Add(iterator.Current);
            }

            if (startIndex == route.Length - 1)
                return list[route[startIndex]];

            if (IsCollection(list[route[startIndex]]))
                return GetCollectionData(list[route[startIndex]], route, startIndex + 1);

            return GetObjectAt(route, list[route[startIndex]], startIndex + 1);
        }

        public static object SetCollectionData(object obj, int[] route, object value, int startIndex = 0)
        {
            if (!IsCollection(obj))
                return null;

            ICollection<object> collection = obj as ICollection<object>;
            List<object> list = new List<object>();
            using (IEnumerator<object> iterator = collection.GetEnumerator())
            {
                while (iterator.MoveNext())
                    list.Add(iterator.Current);
            }


            if (startIndex == route.Length - 1)
            {
                if (route[startIndex] >= list.Count)
                    list.Add(value);
                else
                    list[route[startIndex]] = value;
            }
            else if (IsCollection(list[route[startIndex]]))
                list[route[startIndex]] = SetCollectionData(list[route[startIndex]], route, value, startIndex + 1);
            else
                return SetDataAt(route, value, list[route[startIndex]], startIndex + 1);

            collection = list;
            return collection;
        }
    }
}