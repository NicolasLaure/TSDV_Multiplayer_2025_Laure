using System.Collections.Generic;

namespace Reflection
{
    public class DirtyRegistry
    {
        private Node _registryRoot;
        private List<int[]> dirtyRoutes = new List<int[]>();

        public List<int[]> DirtyRoutes => dirtyRoutes;

        public void SetRegistry(object _baseClass)
        {
            _registryRoot = ReflectionHandler.PopulateTree(_baseClass);
        }

        public void Update(Node root)
        {
            dirtyRoutes.Clear();
            GetDirtyNodes(root, ref dirtyRoutes);
            foreach (int[] route in dirtyRoutes)
                ReflectionHandler.SetDataAt(_registryRoot, route, ReflectionHandler.GetDataAt(root, route));
        }


        public void GetDirtyNodes(Node root, ref List<int[]> dirtyNodesPaths)
        {
            foreach (Node child in root.Children)
            {
                int[] route = child.GetRoute();
                object registryObj = ReflectionHandler.GetDataAt(_registryRoot, route);
                if (registryObj.GetHashCode() != child.nodeObject.GetHashCode())
                    dirtyNodesPaths.Add(child.GetRoute());

                GetDirtyNodes(child, ref dirtyNodesPaths);
            }
        }
    }
}