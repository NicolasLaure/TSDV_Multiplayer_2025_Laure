using Reflection.RPC;
using UnityEngine;

namespace ReflectionTest
{
    public class TestModel
    {
        [RPC]
        private void Original()
        {
            Debug.Log("Original");
        }
    }
}