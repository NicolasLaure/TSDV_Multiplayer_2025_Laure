using System.Collections.Generic;
using UnityEditor.Animations;

namespace Reflection
{
    public class DirtyRegistry
    {
        public static void GetDirtyNodes(Node root, ref List<int[]> dirtyNodesPaths)
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