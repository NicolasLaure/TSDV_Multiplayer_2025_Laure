using System.Collections.Generic;
using UnityEngine;

namespace Network.Factory
{
    [CreateAssetMenu(fileName = "ColorsHandler", menuName = "Factory/ColorHandler", order = 0)]
    public class ColorHandler : ScriptableObject
    {
        public List<Material> materials = new List<Material>();

        public Material GetFromColor(short color)
        {
            return materials[color];
        }
    }
}