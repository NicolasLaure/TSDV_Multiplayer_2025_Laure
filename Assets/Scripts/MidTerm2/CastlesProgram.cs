using Input;
using Network;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesProgram : MonoBehaviour
    {
        [SerializeField] private CastlesView _view;
        [SerializeField] private InputReader _inputReader;
        private CastlesModel _model;

        private ReflectionHandler<CastlesModel> _reflection;

        public void Initialize(NetworkClient client = null)
        {
            _model = new CastlesModel(_inputReader, 0);
            _reflection = new ReflectionHandler<CastlesModel>(ref _model, client);
            _view.InitializeView(_model);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}