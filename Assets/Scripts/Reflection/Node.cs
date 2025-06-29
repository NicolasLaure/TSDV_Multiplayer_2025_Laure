using System;
using System.Collections.Generic;
using Network.Enums;
using UnityEngine;

namespace Reflection
{
    [Serializable]
    public class Node
    {
        private List<Node> children = new List<Node>();
        private Node _parent;
        private bool _shouldSync;
        public object nodeObject;
        private int _lastHash = -1;
        public Attributes attributes = Attributes.None;
        public Node Parent => _parent;

        public bool ContainsSyncedNodes
        {
            get
            {
                foreach (Node child in children)
                {
                    if (child._shouldSync || child.ContainsSyncedNodes)
                        return true;
                }

                return false;
            }
        }

        public bool ShouldSync
        {
            get { return _shouldSync; }
            set
            {
                _shouldSync = value;
                foreach (Node child in children)
                {
                    child.ShouldSync = value;
                }
            }
        }

        public Node[] Children => children.ToArray();

        public Node(object nodeObject)
        {
            this.nodeObject = nodeObject;
            _parent = null;
        }

        public Node(object nodeObject, Node parent)
        {
            this.nodeObject = nodeObject;
            _parent = parent;
            parent.AddChild(this);
        }

        public Node this[int index]
        {
            get { return children[index]; }
        }

        public void AddChild(Node child)
        {
            if (!children.Contains(child))
            {
                children.Add(child);
                if (child.Parent != this)
                    child.SetParent(this);
            }
        }

        public void RemoveChild(Node child)
        {
            children.Remove(child);
        }

        public void SetParent(Node parent)
        {
            _parent = parent;
            parent.AddChild(this);
        }

        public void UpdateValue(object value, bool isRemote)
        {
            nodeObject = value;
            if (isRemote)
                UpdateHash();
        }

        public void UpdateHash()
        {
            _lastHash = nodeObject.GetHashCode();
        }

        public int Count
        {
            get { return children.Count; }
        }

        public int GetChildIndex(Node child)
        {
            if (!children.Contains(child))
                return -1;

            return children.IndexOf(child);
        }

        public int[] GetRoute(List<int> indices = null)
        {
            indices ??= new List<int>();

            if (_parent != null)
            {
                _parent.GetRoute(indices);
                indices.Add(_parent.GetChildIndex(this));
            }

            return indices.ToArray();
        }

        public bool CheckDirty()
        {
            int tmpHash = nodeObject.GetHashCode();
            Debug.Log($"TmpHash: {tmpHash}, LastHash: {_lastHash}");
            bool isDirty = tmpHash != _lastHash;
            _lastHash = tmpHash;
            return isDirty;
        }
    }
}