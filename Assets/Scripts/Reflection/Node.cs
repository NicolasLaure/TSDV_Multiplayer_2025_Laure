using System;
using System.Collections.Generic;

namespace Reflection
{
    [Serializable]
    public class Node
    {
        private List<Node> _children = new List<Node>();
        private Node _parent;
        private bool _shouldSync;
        public object nodeObject;
        public bool isDirty;

        public Node Parent => _parent;

        public bool ContainsSyncedNodes
        {
            get
            {
                foreach (Node child in _children)
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
                foreach (Node child in _children)
                {
                    child.ShouldSync = value;
                }
            }
        }

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
            get { return _children[index]; }
        }

        public void AddChild(Node child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                if (child.Parent != this)
                    child.SetParent(this);
            }
        }

        public void RemoveChild(Node child)
        {
            _children.Remove(child);
        }

        public void SetParent(Node parent)
        {
            _parent = parent;
            parent.AddChild(this);
        }

        public int Count
        {
            get { return _children.Count; }
        }

        public int GetChildIndex(Node child)
        {
            if (!_children.Contains(child))
                return -1;

            return _children.IndexOf(child);
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
    }
}