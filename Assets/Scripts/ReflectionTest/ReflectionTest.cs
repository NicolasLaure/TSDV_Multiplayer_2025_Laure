using MidTerm2;
using Reflection;
using UnityEngine;

namespace ReflectionTest
{
    public class ReflectionTest : MonoBehaviour
    {
        private TestModel model;

        ReflectionHandler<TestModel> reflectionHandler;

        void Start()
        {
            reflectionHandler = new ReflectionHandler<TestModel>(ref model);
        }

        [ContextMenu("Add smsh")]
        void AddSmsh()
        {
            reflectionHandler.SetData(new int[] { 2 }, new TestClass());
        }
    }
}