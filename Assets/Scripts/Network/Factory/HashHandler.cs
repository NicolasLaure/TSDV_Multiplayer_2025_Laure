using System.Collections.Generic;
using UnityEngine;

namespace Network.Factory
{
    [CreateAssetMenu(fileName = "HashHandler", menuName = "Network/HashHandler", order = 0)]
    public class HashHandler : ScriptableObject
    {
        [SerializeField] private List<GameObject> instantiablePrefabs = new List<GameObject>();
        public Dictionary<uint, GameObject> hashToPrefab = new Dictionary<uint, GameObject>();
        public Dictionary<GameObject, uint> prefabToHash = new Dictionary<GameObject, uint>();

        public void Initialize()
        {
            for (uint i = 0; i < instantiablePrefabs.Count; i++)
            {
                hashToPrefab[i] = instantiablePrefabs[(int)i];
                prefabToHash[instantiablePrefabs[(int)i]] = i;
            }
        }
    }
}