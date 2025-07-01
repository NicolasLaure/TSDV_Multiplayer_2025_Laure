using System.Collections.Generic;
using Network;
using UnityEngine;
using Utils;

namespace Reflection
{
    public class DirtyRegistry<ModelType> where ModelType : class, IReflectiveModel
    {
        private List<int[]> dirtyRoutes = new List<int[]>();

        public List<int[]> DirtyRoutes => dirtyRoutes;

        public void Update(Node root)
        {
            dirtyRoutes.Clear();
            GetDirtyNodes(root, ref dirtyRoutes);
        }

        public void GetDirtyNodes(Node root, ref List<int[]> dirtyNodesPaths)
        {
            foreach (Node child in root.Children)
            {
                if (child.CheckDirty())
                {
                    Debug.Log($"Dirty: {Route.RouteString(child.GetRoute())}");
                    dirtyNodesPaths.Add(child.GetRoute());
                }

                GetDirtyNodes(child, ref dirtyNodesPaths);
            }
        }
    }
}