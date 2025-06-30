using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Network_dll.Messages.Data;
using Network.Enums;
using Network.Messages;
using UnityEngine;
using Object = UnityEngine.Object;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    [Serializable]
    public class ReflectionHandler
    {
        private Node root;

        public Node Root => root;

        public void Update(object model)
        {
            root = PopulateTree(model);
        }

        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node(obj);

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Node childNode = new Node(field.GetValue(obj));
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
                        PopulateTree(item, childNode);
                    }
                }
                else if (!field.FieldType.IsPrimitive)
                {
                    PopulateTree(field.GetValue(obj), childNode);
                }

                if (childNode.ContainsSyncedNodes)
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

        public static object GetDataAt(Node root, int[] route)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            return target.nodeObject;
        }

        public PrimitiveMessage GetMessage(int[] route)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            PrimitiveData data;
            data.type = GetObjectType(target.nodeObject);
            data.routeLength = route.Length;
            data.route = route;
            data.obj = target.nodeObject;
            return new PrimitiveMessage(data, target.attributes);
        }

        public PrimitiveType GetObjectType(object obj)
        {
            TypeCode objType = Convert.GetTypeCode(obj);
            PrimitiveType primitiveType;
            switch (objType)
            {
                case TypeCode.Boolean:
                    primitiveType = PrimitiveType.TypeBool;
                    break;
                case TypeCode.Byte:
                    primitiveType = PrimitiveType.TypeByte;
                    break;
                case TypeCode.SByte:
                    primitiveType = PrimitiveType.TypeSbyte;
                    break;
                case TypeCode.Int16:
                    primitiveType = PrimitiveType.TypeShort;
                    break;
                case TypeCode.UInt16:
                    primitiveType = PrimitiveType.TypeUshort;
                    break;
                case TypeCode.Int32:
                    primitiveType = PrimitiveType.TypeInt;
                    break;
                case TypeCode.UInt32:
                    primitiveType = PrimitiveType.TypeUint;
                    break;
                case TypeCode.Int64:
                    primitiveType = PrimitiveType.TypeLong;
                    break;
                case TypeCode.UInt64:
                    primitiveType = PrimitiveType.TypeUlong;
                    break;
                case TypeCode.Single:
                    primitiveType = PrimitiveType.TypeFloat;
                    break;
                case TypeCode.Double:
                    primitiveType = PrimitiveType.TypeDouble;
                    break;
                case TypeCode.Decimal:
                    primitiveType = PrimitiveType.TypeDecimal;
                    break;
                case TypeCode.Char:
                    primitiveType = PrimitiveType.TypeChar;
                    break;
                case TypeCode.String:
                    primitiveType = PrimitiveType.TypeString;
                    break;
                default:
                    primitiveType = PrimitiveType.NonPrimitive;
                    break;
            }

            return primitiveType;
        }

        public static void SetDataAt(Node root, int[] route, object value)
        {
            Node target = root;
            for (int i = 0; i < route.Length; i++)
            {
                target = target[route[i]];
            }

            Debug.Log($"New data at{RouteString(route)}: {value}");
            target.UpdateValue(value);
        }

        private static string RouteString(int[] route)
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