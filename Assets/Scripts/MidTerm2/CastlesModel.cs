using System;
using CustomMath;
using Input;
using Network.Enums;
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
        [Sync] private sbyte testSByte = 0;
        [Sync] private byte testByte = 0;
        [Sync] private short testShort = 0;
        [Sync] private ushort testUShort = 0;
        [Sync] private int testInt = 0;
        [Sync] private uint testUInt = 0;
        [Sync] private long testLong = 0;
        [Sync] private ulong testULong = 0;
        [Sync] private float testFloat = 0;
        [Sync] private double testDouble = 0;
        [Sync] private decimal testDecimal = 0;
        [Sync] private bool testBool = false;
        [Sync] private char testChar = ' ';
        [Sync] private string testString = "";
        public Vec3 position;
        private TestModel test = new TestModel();

        public CastlesModel(InputReader input)
        {
            position = new Vec3(0.0f, 0.0f, 0.0f);
            input.onMove += Move;
            input.onShoot += Test;
        }

        private void Move(Vec3 vec3)
        {
            position += vec3;
        }

        public void TestRpc()
        {
            Debug.Log("Some Method");
        }

        public void Test()
        {
            testSByte = 10;
            testByte = 4;
            testShort = -2342;
            testUShort = 32423;
            testInt = -312312312;
            testUInt = 3123;
            testLong = 4234234234234;
            testULong = 423432;
            testFloat = 23.0234423f;
            testDouble = 1312413.0234423d;
            testDecimal = 12.6000053465m;
            testBool = true;
            testChar = '漢';
            testString = "漢字漢字";
        }
    }
}