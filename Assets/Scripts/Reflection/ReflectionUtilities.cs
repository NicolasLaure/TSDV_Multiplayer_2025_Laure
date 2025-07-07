using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Network.Enums;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    public static class ReflectionUtilities
    {
        public static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node();
            if (obj == null)
                return root;

            foreach (FieldInfo field in obj.GetType().GetFields(bindingFlags))
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
            if (startIndex >= route.Length - 1)
                return obj;

            FieldInfo info = obj.GetType().GetFields(bindingFlags)[route[startIndex]];
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
            if (model == null)
                return false;

            int count = 0;
            foreach (FieldInfo field in model.GetType().GetFields(bindingFlags))
            {
                if (ReferenceEquals(field.GetValue(model), obj))
                {
                    route.Insert(0, count);
                    return true;
                }

                if (IsCollection(field))
                {
                    if (GetCollectionRoute(field.GetValue(model), obj, route))
                    {
                        route.Insert(0, count);
                        return true;
                    }
                }
                else if (!field.FieldType.IsPrimitive && TryGetRoute(field.GetValue(model), obj, route))
                {
                    route.Insert(0, count);
                    return true;
                }

                count++;
            }

            return false;
        }

        public static bool GetCollectionRoute(object model, object obj, List<int> route)
        {
            if (model == null || !IsCollection(model) || (model as ICollection) == null)
                return false;

            int count = 0;
            foreach (object item in model as ICollection)
            {
                if (item == null)
                    continue;

                if (ReferenceEquals(item, obj))
                {
                    route.Insert(0, count);
                    return true;
                }

                if (IsCollection(item))
                {
                    if (GetCollectionRoute(item, obj, route))
                    {
                        route.Insert(0, count);
                        return true;
                    }
                }
                else if (!item.GetType().IsPrimitive && item.GetType() == obj.GetType() && TryGetRoute(item, obj, route))
                {
                    route.Insert(0, count);
                    return true;
                }

                count++;
            }

            return false;
        }

        public static Type GetCollectionType(Type obj)
        {
            Type elementType;
            if (obj.IsGenericType)
                elementType = obj.GetGenericArguments()[0];
            else
                elementType = obj.GetElementType();

            if (elementType.IsArray || typeof(ICollection).IsAssignableFrom(elementType))
                return GetCollectionType(elementType);

            return elementType;
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
    }
}