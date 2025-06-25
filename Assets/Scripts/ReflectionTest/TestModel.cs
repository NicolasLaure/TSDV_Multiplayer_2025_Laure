using System.Collections.Generic;
using System.Numerics;

public class TestClass
{
    private int a = 123;
    private float f = 123.231f;

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
    private Vector3 vec3 = new Vector3(3, 1, 0);
    private int[] ints = { 1, 5, 3 };
    private TestClass testClass = new TestClass();
    // private TestClass[] testClasses = { new TestClass(30), new TestClass(12) };
    // private List<List<int>> _listofList = new List<List<int>>();

    public TestModel()
    {
    }
}