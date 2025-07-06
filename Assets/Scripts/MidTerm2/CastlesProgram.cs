using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Network;
using Network.Factory;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesProgram : MonoBehaviour
    {
        [SerializeField] private CastlesView _view;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private ColorHandler _colorHandler;
        [SerializeField] private HashHandler prefabHashHandler;
        private CastlesModel _model;

        public ReflectionHandler<CastlesModel> reflection;
        private ReflectiveClient<CastlesModel> _client;

        public void Initialize(ReflectiveClient<CastlesModel> client = null)
        {
            prefabHashHandler.Initialize();
            _model = new CastlesModel(_inputReader, client);
            reflection = new ReflectionHandler<CastlesModel>(ref _model, client);
            List<Type> types = new List<Type>();
            types.Add(typeof(Castle));
            types.Add(typeof(Warrior));

            _client = client;
            _client.onHandshakeOk += _model.Initialize;
            client.factory = new ReflectiveFactory<CastlesModel>(reflection, types, _colorHandler, prefabHashHandler);
            client.reflection = reflection;
            _view.InitializeView(_model);
        }

        private void Update()
        {
            reflection.Update();
        }
    }
}