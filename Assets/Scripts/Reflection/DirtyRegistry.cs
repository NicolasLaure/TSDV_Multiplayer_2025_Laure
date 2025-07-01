using System.Collections.Generic;
using Network;

namespace Reflection
{
    public class DirtyRegistry<ModelType> where ModelType : class, IReflectiveModel
    {
        private Node _registryRoot;
        private List<int[]> dirtyRoutes = new List<int[]>();

        public List<int[]> DirtyRoutes => dirtyRoutes;

        public void SetRegistry(object _baseClass)
        {
            _registryRoot = ReflectionHandler<ModelType>.PopulateTree(_baseClass);
        }

        // public void Update(Node root)
        // {
        //     dirtyRoutes.Clear();
        //     GetDirtyNodes(root, ref dirtyRoutes);
        //     foreach (int[] route in dirtyRoutes)
        //         ReflectionHandler<ModelType>.SetDataAt(_registryRoot, route, ReflectionHandler<ModelType>.GetDataAt(root, route));
        // }

        public void GetDirtyNodes(Node root, ref List<int[]> dirtyNodesPaths)
        {
            foreach (Node child in root.Children)
            {
                if (child.CheckDirty())
                    dirtyNodesPaths.Add(child.GetRoute());

                GetDirtyNodes(child, ref dirtyNodesPaths);
            }
        }
    }
}