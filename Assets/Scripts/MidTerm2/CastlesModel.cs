using System;
using CustomMath;
using Input;
using Reflection;
using Reflection.RPC;
using ReflectionTest;
using UnityEngine;
using UnityEngine.Serialization;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel : IReflectiveModel
    {
        [Sync] private float a = 0;
        [Sync] public Vec3 position;
        private TestModel test = new TestModel();

        public CastlesModel(InputReader input)
        {
            position = new Vec3(0.0f, 0.0f, 0.0f);
            input.onMove += Move;
        }

        private void Move(Vec3 vec3)
        {
            position += vec3;
        }

        [RPC]
        public void Test()
        {
            Debug.Log("Sending Something");
        }
    }
}