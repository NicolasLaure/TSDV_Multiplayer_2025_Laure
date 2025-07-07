using System;
using System.Collections.Generic;
using Reflection;
using UnityEngine;

namespace ReflectionTest
{
    [Serializable]
    public class TestModel : IReflectiveModel
    {
        [Sync] public int a;
        private float b;
        List<TestClass> c = new List<TestClass>();

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