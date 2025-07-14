using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Network;
using Network.Factory;
using Reflection;

namespace MidTerm2
{
    public class CastlesProgram
    {
        private CastlesView _view;
        private ColorHandler _colorHandler;
        private HashHandler prefabHashHandler;
        private CastlesModel _model;

        public ReflectionHandler<CastlesModel> reflection;
        private ReflectiveClient<CastlesModel> _client;

        public CastlesProgram(ColorHandler color, HashHandler hash)
        {
            _view = CastlesView.Instance;
            _colorHandler = color;
            prefabHashHandler = hash;
        }

        public void Initialize(ReflectiveClient<CastlesModel> client = null, bool isAuth = false)
        {
            prefabHashHandler.Initialize();
            _model = new CastlesModel();
            reflection = new ReflectionHandler<CastlesModel>(ref _model, client);
            List<Type> types = new List<Type>();
            types.Add(typeof(Castle));
            types.Add(typeof(Warrior));

            _client = client;
            _client.onHandshakeOk += InitializeModel;

            client.factory = new ReflectiveFactory<CastlesModel>(reflection, types, _colorHandler, prefabHashHandler);
            client.reflection = reflection;

            _view.InitializeView(_model, client.Id);

            if (!isAuth)
            {
                CastlesController.Instance.model = _model;
                CastlesController.Instance.factory = client.factory;
                CastlesController.Instance.reflection = reflection;
            }
        }

        public void Update()
        {
            reflection.Update();
            _model.Update();
        }

        private void InitializeModel()
        {
            _model.Initialize();
            _model.SetArmy(_client.Id == 0);
        }
    }
}