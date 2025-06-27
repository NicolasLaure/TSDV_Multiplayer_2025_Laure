using System.Collections.Generic;
using System.Numerics;
using Reflection;

namespace ReflectionTest
{
    public class TestClass
    {
        private int a = 123;
        private float f = 123.231f;
        [Sync] private Vector3 vec3 = new Vector3(12, 3, 4);

        public TestClass()
        {
        }

        public TestClass(int a)
        {
            this.a = a;
        }
    }

    public class TestModel
    {
        private int qty = 0;
        // WARNING VECTOR3 INITIALIZED WITH 2 VALUES NOT SAVING THIRD
        [Sync] private Vector3 vec3 = new Vector3(3, 1, 2);
        private int[] ints = { 1, 5, 3 };
        private TestClass testClass = new TestClass();
        // private TestClass[] testClasses = { new TestClass(30), new TestClass(12) };
        private List<List<int>> _listofList = new List<List<int>>();

        public TestModel()
        {
        }
    }
}