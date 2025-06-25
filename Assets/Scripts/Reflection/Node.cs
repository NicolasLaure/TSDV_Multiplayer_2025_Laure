using System.Collections.Generic;

namespace Reflection
{
    public class Node
    {
        private List<Node> _children = new List<Node>();
        private Node _parent;
        public object nodeObject;
        public bool isDirty;

        public Node(object nodeObject)
        {
            this.nodeObject = nodeObject;
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
            _children.Add(child);
            child.AddParent(this);
        }

        public void RemoveChild(Node child)
        {
            _children.Remove(child);
        }

        public void AddParent(Node parent)
        {
            _parent = parent;
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

        public int[] GetRoute()
        {
            List<int> indices = new List<int>();
            if (_parent != null)
            {
                indices.AddRange(_parent.GetRoute());
                indices.Add(_parent.GetChildIndex(this));
            }

            return indices.ToArray();
        }
    }
}