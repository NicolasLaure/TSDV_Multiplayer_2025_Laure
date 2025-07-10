using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Network.Enums;
using UnityEditor;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    public static class ReflectionUtilities
    {
        public static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static BindingFlags genericStaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private static int StartIndex = 0;

        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node();
            if (obj == null)
                return root;

            if (obj.HasBaseClass())
            {
                typeof(ReflectionUtilities).GetMethod(nameof(PopulateParents), genericStaticFlags).MakeGenericMethod(
                obj.GetType().BaseType).Invoke(null, new[] { obj, root });
            }

            foreach (FieldInfo field in obj.GetType().GetFields(bindingFlags))
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

                if (IsCollection(field) && field.GetValue(obj) != null)
                    PopulateCollection(childNode, field.GetValue(obj));
                else if (!field.FieldType.IsPrimitive && !field.IsDelegate())
                    PopulateTree(field.GetValue(obj), childNode);

                if (!childNode.ContainsSyncedNodes)
                    childNode.RemoveAllChildren();

                childNode.SetParent(root);
            }

            return root;
        }

        public static void PopulateParents<T>(object obj, Node root) where T : class
        {
            if (!typeof(T).BaseType.IsInterface && typeof(T).BaseType != typeof(object))
            {
                Type baseType = typeof(T).BaseType;
            }

            foreach (FieldInfo field in typeof(T).GetFields(bindingFlags))
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

                if (IsCollection(field) && field.GetValue(obj) != null)
                    PopulateCollection(childNode, field.GetValue(obj));
                else if (!field.FieldType.IsPrimitive && !field.IsDelegate())
                    PopulateTree(field.GetValue(obj), childNode);

                if (!childNode.ContainsSyncedNodes)
                    childNode.RemoveAllChildren();

                childNode.SetParent(root);
            }
        }

        public static object GetObjectAt(int[] route, object obj, int startIndex = 0)
        {
            if (startIndex != 0 && startIndex >= route.Length - 1)
            {
                if (obj.HasBaseClass())
                {
                    return typeof(ReflectionUtilities).GetMethod(nameof(GetParentObjectAt), genericStaticFlags).MakeGenericMethod(
                    obj.GetType().BaseType).Invoke(null, new[] { route, obj, startIndex });
                }
            }

            if (obj.HasBaseClass())
            {
                return typeof(ReflectionUtilities).GetMethod(nameof(GetParentObjectAt), genericStaticFlags).MakeGenericMethod(
                obj.GetType().BaseType).Invoke(null, new[] { route, obj, startIndex });
            }

            FieldInfo info = obj.GetType().GetFields(bindingFlags)[route[startIndex]];
            if (route.Length == 1)
            {
                startIndex++;
                return GetObjectAt(route, info.GetValue(obj), startIndex);
            }

            if (info.FieldType != typeof(string) && (info.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(info.FieldType)))
            {
                int index = 0;
                foreach (object item in info.GetValue(obj) as ICollection)
                {
                    Debug.Log($"Route:{Route.RouteString(route)}, Length: {route.Length}, index: {startIndex + 1}");
                    if (index == route[startIndex + 1])
                    {
                        return GetObjectAt(route, item, startIndex + 2);
                    }

                    index++;
                }

                return null;
            }

            return GetObjectAt(route, info.GetValue(obj), startIndex + 1);
        }

        public static object GetObjectAt<T>(int[] route, object obj, int startIndex = 0)
        {
            if (startIndex != 0 && startIndex >= route.Length - 1)
                return obj;

            FieldInfo info = obj.GetType().GetFields(bindingFlags)[route[startIndex]];
            if (route.Length == 1)
            {
                startIndex++;
                return GetObjectAt<T>(route, info.GetValue(obj), startIndex);
            }

            if (info.FieldType != typeof(string) && (info.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(info.FieldType)))
            {
                int index = 0;
                foreach (object item in info.GetValue(obj) as ICollection)
                {
                    Debug.Log($"Route:{Route.RouteString(route)}, Length: {route.Length}, index: {startIndex + 1}");
                    if (index == route[startIndex + 1])
                    {
                        return GetObjectAt<T>(route, item, startIndex + 2);
                    }

                    index++;
                }

                return null;
            }

            return GetObjectAt<T>(route, info.GetValue(obj), startIndex + 1);
        }

        public static object GetParentObjectAt<T>(int[] route, object obj, int startIndex) where T : class
        {
            FieldInfo info;
            if (startIndex > route.Length - 1)
                return obj;

            info = typeof(T).GetFields(bindingFlags)[route[startIndex]];
            if (startIndex != 0 && startIndex == route.Length - 1)
            {
                if (typeof(T).HasBaseClass())
                {
                    return typeof(ReflectionUtilities).GetMethod(nameof(GetParentObjectAt), genericStaticFlags).MakeGenericMethod(
                    typeof(T).BaseType).Invoke(null, new[] { route, obj, startIndex });
                }

                return info.GetValue(obj);
            }

            if (route.Length == 1)
            {
                startIndex++;
                return GetObjectAt<T>(route, info.GetValue(obj), startIndex);
            }

            if (info.FieldType != typeof(string) && (info.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(info.FieldType)))
            {
                int index = 0;
                foreach (object item in info.GetValue(obj) as ICollection)
                {
                    if (index == route[startIndex + 1])
                    {
                        return GetObjectAt<T>(route, item, startIndex + 2);
                    }

                    index++;
                }

                return null;
            }

            return GetObjectAt<T>(route, info.GetValue(obj), startIndex + 1);
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
                else if (!field.FieldType.IsPrimitive && !field.IsDelegate() && TryGetRoute(field.GetValue(model), obj, route))
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
                else if (!item.GetType().IsPrimitive && !item.IsDelegate() && item.GetType() == obj.GetType() && TryGetRoute(item, obj, route))
                {
                    route.Insert(0, count);
                    return true;
                }

                count++;
            }

            return false;
        }

        public static Type GetCollectionType(this Type obj)
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

        public static bool IsCollection(this object obj)
        {
            return obj.GetType() != typeof(string) && (obj.GetType().IsArray || obj is ICollection);
        }

        public static bool IsCollection(this FieldInfo field)
        {
            return field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType));
        }

        public static bool IsDelegate(this FieldInfo field)
        {
            return field.FieldType == typeof(Action) || field.FieldType == typeof(Action<>);
        }

        public static bool IsDelegate(this object obj)
        {
            return obj.GetType() == typeof(Action) || obj.GetType() == typeof(Action<>);
        }

        public static bool HasBaseClass(this object obj)
        {
            return !obj.GetType().BaseType.IsInterface && obj.GetType().BaseType != typeof(object);
        }

        public static bool HasBaseClass(this Type type)
        {
            return !type.BaseType.IsInterface && type.BaseType != typeof(object);
        }

        public static int GetBaseMaxIndex(this Type type)
        {
            int count = 0;
            if (type.HasBaseClass())
                count += type.BaseType.GetBaseMaxIndex();

            return type.GetFields(bindingFlags).Length - 1;
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

            startIndex++;
            return GetObjectAt(route, list[route[startIndex]], startIndex);
        }
    }
}