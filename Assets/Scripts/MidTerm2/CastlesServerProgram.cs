using System;
using System.Collections.Generic;
using FPS.AuthServer;
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
        private ColorHandler _colorHandler;
        private HashHandler prefabHashHandler;
        public CastlesModel _model;

        public ReflectionHandler<CastlesModel> reflection;
        private ReflectiveAuthoritativeServer<CastlesModel> _server;

        public CastlesServerProgram(ColorHandler colorHandler, HashHandler hash)
        {
            _view = CastlesView.Instance;
            _colorHandler = colorHandler;
            prefabHashHandler = hash;
        }

        public void Initialize(ReflectiveAuthoritativeServer<CastlesModel> server = null)
        {
            prefabHashHandler.Initialize();
            _model = new CastlesModel();
            reflection = new ReflectionHandler<CastlesModel>(ref _model, server);
            _model.Initialize();
            List<Type> types = new List<Type>();
            types.Add(typeof(Castle));
            types.Add(typeof(Warrior));

            _server = server;
            _server.factory = new ReflectiveServerFactory<CastlesModel>(reflection, types, _colorHandler, prefabHashHandler);
            _server.reflection = reflection;

            _view.InitializeView(_model);

            AuthServerController.Instance.model = _model;
            AuthServerController.Instance.factory = server.factory;
            AuthServerController.Instance.reflection = reflection;
        }

        public void Update()
        {
            reflection.Update();
            _model.Update();
        }
    }
}