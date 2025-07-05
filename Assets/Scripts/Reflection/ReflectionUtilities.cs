using System.Collections;
using System.Reflection;
using Network.Enums;
using UnityEngine;
using Utils;
using PrimitiveType = Network.Enums.PrimitiveType;

namespace Reflection
{
    public static class ReflectionUtilities
    {
        public static Node PopulateTree(object obj, Node root = null)
        {
            root ??= new Node();
            if (obj == null)
                return root;

            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
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

                if (IsCollection(field))
                    PopulateCollection(childNode, field.GetValue(obj));
                else if (!field.FieldType.IsPrimitive)
                    PopulateTree(field.GetValue(obj), childNode);

                if (!childNode.ContainsSyncedNodes)
                    childNode.RemoveAllChildren();

                childNode.SetParent(root);
            }

            return root;
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
        
    }
}