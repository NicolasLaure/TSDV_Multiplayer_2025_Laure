using System.Numerics;

public class TestClass
{
    private int a = 123;
    private float f = 123.231f;

    public TestClass()
    {
    }
}

public class TestModel
{
    private int qty = 0;
    private Vector3 vec3 = new Vector3(3, 1, 0);
    private int[] ints = new int[] { 1, 5, 3 };
    private TestClass testClass = new TestClass();

    public TestModel()
    {
    }
}