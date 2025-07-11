using System;
using System.Collections.Generic;
using Network.Enums;
using UnityEngine;
using Utils;

namespace Reflection
{
    [Serializable]
    public class Node
    {
        private List<Node> children = new List<Node>();
        private Node _parent;
        private bool _shouldSync;
        public Attributes attributes = Attributes.None;
        private int ownerId = -1;
        public int lastHash = -1;
        public int currentHash;
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

        public int OwnerId
        {
            get => ownerId;
            set
            {
                ownerId = value;
                foreach (Node child in children)
                {
                    child.OwnerId = value;
                }
            }
        }

        public Node[] Children => children.ToArray();

        public Node()
        {
            _parent = null;
        }

        public Node(Node parent)
        {
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

        public void RemoveAllChildren()
        {
            children.Clear();
        }


        public void SetParent(Node parent)
        {
            _parent = parent;
            parent.AddChild(this);
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

        public bool CheckDirty(int clientId)
        {
            if (clientId != ownerId)
                return false;

            bool isDirty = currentHash != lastHash;
            lastHash = currentHash;
            return isDirty;
        }
    }
}