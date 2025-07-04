using System;
using System.Collections.Generic;
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
        private sbyte testSByte = 0;
        private byte testByte = 0;
        private short testShort = 0;
        private ushort testUShort = 0;
        private int testInt = 0;
        private uint testUInt = 0;
        private long testLong = 0;
        private ulong testULong = 0;
        private float testFloat = 0;
        private double testDouble = 0;
        private decimal testDecimal = 0;
        private bool testBool = false;
        private char testChar = ' ';
        private string testString = "";
        public Vec3 position;
        public TestModel test = new TestModel(2, 3);
        public List<TestModel> tests = new List<TestModel>();

        public CastlesModel(InputReader input)
        {
            position = new Vec3(0.0f, 0.0f, 0.0f);
            input.onMove += Move;
            input.onShoot += Test;
            tests.Add(new TestModel(2, 31.2f));
            tests.Add(new TestModel());
        }

        private void Move(Vec3 vec3)
        {
            position += vec3;
        }

        [RPC]
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