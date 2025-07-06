using System;
using System.Collections.Generic;

namespace Network.Factory.Reflection
{
    public class ReflectiveHashHandler
    {
        private List<Type> _instantiableTypes;
        public Dictionary<uint, Type> hashToType = new Dictionary<uint, Type>();
        public Dictionary<Type, uint> typeToHash = new Dictionary<Type, uint>();

        public ReflectiveHashHandler(List<Type> list)
        {
            _instantiableTypes = list;
            Initialize();
        }

        private void Initialize()
        {
            for (uint i = 0; i < _instantiableTypes.Count; i++)
            {
                hashToType[i] = _instantiableTypes[(int)i];
                typeToHash[_instantiableTypes[(int)i]] = i;
            }
        }
    }
}