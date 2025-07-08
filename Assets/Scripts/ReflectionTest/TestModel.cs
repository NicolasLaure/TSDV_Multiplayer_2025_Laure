using System;
using System.Collections.Generic;
using MidTerm2.Model;
using Reflection;
using UnityEngine;

namespace ReflectionTest
{
    [Serializable]
    public class TestModel : IReflectiveModel
    {
        [Sync] public int a;
        private float b;
        List<Warrior> warriors = new List<Warrior>();

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