using System.Collections.Generic;
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
        private ReflectionHandler<CastlesModel> _reflection;

        void Start()
        {
            _model = new CastlesModel(_inputReader);
            _reflection = new ReflectionHandler<CastlesModel>(ref _model);
        }

        void Update()
        {
            _reflection.Update();
        }

        [ContextMenu("Test")]
        private void TestManualSetting()
        {
            Debug.Log($"Reflection _model.position.x = {_reflection.GetDataAt(new int[] { 1, 0 })}");
            _model.position.x = 3;
            Debug.Log($"Reflection _model.position.x = {_reflection.GetDataAt(new int[] { 1, 0 })}");
            // _reflection.SetDataAt(new int[] { 1, 1 }, 2, _model);
        }
    }
}