using System;
using UnityEngine;

namespace ReflectionTest
{
    [Serializable]
    public class TestClass
    {
        public int papa = 0;

        public TestClass()
        {
            papa = 10;
            Debug.Log("Created New TestClass");
        }
    }
}