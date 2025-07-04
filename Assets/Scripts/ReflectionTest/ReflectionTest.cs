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
        private void Test()
        {
            _model.test.a = 12;
            _reflection.SetDataAt(new int[] { 16, 1, 0 }, 11, _reflection._model);
        }
    }
}