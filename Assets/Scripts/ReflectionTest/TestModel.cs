using System;
using Reflection;
using UnityEngine;

namespace ReflectionTest
{
    [Serializable]
    public class TestModel
    {
        [Sync] public int a;
        private float b;

        public TestModel(int a, float b)
        {
            this.a = a;
            this.b = b;
        }

        public TestModel()
        {
            a = 1;
            b = 0.1f;
        }

        private void Original()
        {
            Debug.Log($"Original : {a}");
        }
    }
}