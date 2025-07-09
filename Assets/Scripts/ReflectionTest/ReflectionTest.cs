using MidTerm2;
using MidTerm2.Model;
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

        private int index = 0;

        [ContextMenu("Add smsh")]
        void AddSmsh()
        {
            reflectionHandler.SetData<Warrior>(new int[] { 2, index }, new Warrior());
            index++;
        }

        [ContextMenu("Modify")]
        void Modify()
        {
            reflectionHandler.SetData<Warrior>(new int[] { 2, 1, 0 }, 30);
        }
    }
}