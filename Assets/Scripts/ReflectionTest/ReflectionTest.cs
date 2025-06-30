using Input;
using MidTerm2;
using Reflection;
using UnityEngine;

namespace ReflectionTest
{
    public class ReflectionTest : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        private CastlesModel _model;
        private ReflectionHandler _reflection;
        private DirtyRegistry _registry;

        void Start()
        {
            _model = new CastlesModel(_inputReader);
            _registry = new DirtyRegistry();
            _reflection = new ReflectionHandler();
            _registry.SetRegistry(_model);
        }

        void Update()
        {
            _reflection.Update(_model);
            _registry.Update(_reflection.Root);
            Debug.Log($"Float at [1][1]: {ReflectionHandler.GetDataAt(_reflection.Root, new int[] { 1, 1 })}");
        }

        [ContextMenu("Test")]
        private void TestManualSetting()
        {
            _model.position.x = 3;
            //_reflection.SetDataAt(new int[] { 1, 1 }, 2, false);
        }
    }
}