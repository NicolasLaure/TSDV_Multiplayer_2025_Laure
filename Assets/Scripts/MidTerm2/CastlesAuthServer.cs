using System;
using System.Net;
using Network;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesAuthServer : MonoBehaviourSingleton<CastlesAuthServer>
    {
        [SerializeField] private ColorHandler color;
        [SerializeField] private HashHandler hashHandler;

        public ReflectiveAuthoritativeServer<CastlesModel> _server;
        public ReflectionHandler<CastlesModel> _reflectionHandler;
        public ReflectiveServerFactory<CastlesModel> factory;
        public Action<Tuple<Vector2, int>> onMouseClick;

        private CastlesServerProgram _castlesServerProgram;

        private void Start()
        {
            _server = new ReflectiveAuthoritativeServer<CastlesModel>();
            string[] args;
#if UNITY_EDITOR
            args = new[] { " ",_server.defaultPort.ToString() };
#else
             args = Environment.GetCommandLineArgs();
#endif
            _server.Start(int.Parse(args[1]));
            _server.onNewClient += HandleHandshake;
            _server.OnReceiveEvent += OnReceiveDataEvent;
            _castlesServerProgram = new CastlesServerProgram(color, hashHandler);
            _castlesServerProgram.Initialize(_server);
            AuthServerController.Instance.StartUp();
        }

        private void Update()
        {
            _server.Update();
            _castlesServerProgram.Update();
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = _server.GetReceivedClientId(ip);
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            switch (messageType)
            {
                case MessageType.PrivateHandshake:
                    PrivateHandshake privateHandshake = new PrivateHandshake(data);
                    HandlePrivateHandshake(privateHandshake, receivedClientId);
                    break;
                case MessageType.MouseInput:
                    MouseInput mouseInput = new MouseClickMessage(data).input;
                    Vector2 mousePos = new Vector2(mouseInput.x, mouseInput.y);
                    onMouseClick?.Invoke(new Tuple<Vector2, int>(mousePos, receivedClientId));
                    break;

                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleHandshake(int clientId)
        {
            ServerHsResponse hsResponse = new ServerHsResponse(clientId, _server.Seed);
            _server.SendToClient(hsResponse.Serialize(), clientId);
        }

        private void HandlePrivateHandshake(PrivateHandshake privateHandshake, int id)
        {
            _castlesServerProgram._model.SetServerArmy(privateHandshake.clientId, privateHandshake.clientId == 0, privateHandshake.color);
        }

        public void Instantiate(GameObject prefab, Matrix4x4 trs, short instanceColor, int clientId)
        {
            if (!hashHandler.prefabToHash.ContainsKey(prefab))
            {
                Debug.Log("Invalid Prefab");
                return;
            }

            InstanceData instanceData = new InstanceData
            {
                originalClientID = clientId,
                prefabHash = hashHandler.prefabToHash[prefab],
                instanceID = -1,
                trs = ByteFormat.Get4X4Bytes(trs),
                color = instanceColor
            };

            //_serverFactory.Instantiate(instanceData);
        }
    }
}