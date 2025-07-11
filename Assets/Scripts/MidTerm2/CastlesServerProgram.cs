using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Network;
using Network.Factory;
using Reflection;

namespace MidTerm2
{
    public class CastlesServerProgram
    {
        private CastlesView _view;
        private InputReader _inputReader;
        private ColorHandler _colorHandler;
        private HashHandler prefabHashHandler;
        private CastlesModel _model;

        public ReflectionHandler<CastlesModel> reflection;
        private ReflectiveClient<CastlesModel> _client;

        public CastlesServerProgram(ColorHandler color, HashHandler hash)
        {
            _view = CastlesView.Instance;
            _inputReader = InputReader.Instance;
            _colorHandler = color;
            prefabHashHandler = hash;
        }

        public void Initialize(ReflectiveClient<CastlesModel> client = null)
        {
            prefabHashHandler.Initialize();
            _model = new CastlesModel(_inputReader, client, true);
            reflection = new ReflectionHandler<CastlesModel>(ref _model, client);
            List<Type> types = new List<Type>();
            types.Add(typeof(Castle));
            types.Add(typeof(Warrior));

            _client = client;
            _client.onHandshakeOk += _model.Initialize;
            client.factory = new ReflectiveFactory<CastlesModel>(reflection, types, _colorHandler, prefabHashHandler);
            client.reflection = reflection;

            _view.InitializeView(_model);

            CastlesController.Instance.model = _model;
            CastlesController.Instance.factory = client.factory;
            CastlesController.Instance.reflection = reflection;
        }

        public void Update()
        {
            reflection.Update();
        }
    }
}