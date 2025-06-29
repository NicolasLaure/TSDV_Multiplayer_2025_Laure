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

        void Start()
        {
            _model = new CastlesModel(_inputReader);
            _reflection = new ReflectionHandler();
            _reflection._model = _model;
            _reflection.Start();
        }

        void Update()
        {
            _reflection.Update();
            Debug.Log($"Float at [1][1]: {_reflection.GetDataAt(new int[] { 1, 1 })}");
        }

        [ContextMenu("Test")]
        private void TestManualSetting()
        {
            _model.position.x = 3;
            //_reflection.SetDataAt(new int[] { 1, 1 }, 2, false);
        }
    }
}