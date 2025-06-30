using System;
using CustomMath;
using Input;
using Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel
    {
        [Sync] private float a = 0;
        [Sync] public Vec3 position;

        public CastlesModel(InputReader input)
        {
            position = new Vec3(0.0f, 0.0f, 0.0f);
            input.onMove += Move;
        }

        private void Move(Vec3 vec3)
        {
            a += vec3.x;
        }

        [RPC]
        private void Test()
        {
            Debug.Log("Sending Something");
        }
    }
}