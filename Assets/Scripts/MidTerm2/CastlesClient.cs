using System;
using System.Net;
using Input;
using Network;
using Network_dll.Messages.Data;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using Reflection;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidTerm2
{
    public class CastlesClient : MonoBehaviourSingleton<CastlesClient>
    {
        [SerializeField] private HashHandler prefabsData;

        [SerializeField] private ErrorMessagePanel errorPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private InputReader _inputReader;
        public int clientId = -1;

        private NetworkClient _networkClient;
        private ClientFactory _clientFactory;

        private CastlesModel _model;
        private CastlesView _view;
        private ReflectionHandler _reflection;
        //[SerializeField] private CastlesController input;

        private void Start()
        {
            prefabsData.Initialize();
            //_clientFactory = new ClientFactory(prefabsData, colorHandler);
            //onEntityUpdated.AddListener(OnEntityUpdate);

            _networkClient = ClientManager.Instance.networkClient;

            _networkClient.onClientStart += OnClientStarted;
            _networkClient.OnReceiveEvent += OnReceiveDataEvent;
            _networkClient.onDisconnection += HandleAbruptDisconnection;
            _networkClient.onClientDisconnect += HandleDisconnectedUser;
            _networkClient.onError += HandleError;

            InputReader.Instance.onQuit += HandleQuit;
        }

        private void InitializeModel()
        {
            _model = new CastlesModel(_inputReader);
            _reflection = new ReflectionHandler();
            _reflection._model = _model;
            _reflection.Start();
        }

        private void Update()
        {
            if (_reflection != null)
            {
                _reflection.Update();
                SendDirtyValues();
            }
        }

        private void OnDestroy()
        {
            HandleQuit();
            if (Instance == this)
                Instance = null;
        }

        private void OnClientStarted()
        {
            if (_networkClient.port != _networkClient.defaultPort)
                InitializeModel();
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
        {
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            switch (messageType)
            {
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponseData(new ServerHsResponse(data));
                    break;
                case MessageType.PrivateHsResponse:
                    HandlePrivateHsResponseData(new PrivateServerHsResponse(data));
                    break;
                case MessageType.InstantiateRequest:
                    _clientFactory.Instantiate(new InstantiateRequest(data).instanceData);
                    break;
                case MessageType.DeInstantiateRequest:
                    DeInstantiateRequest request = new DeInstantiateRequest(data);
                    _clientFactory.DeInstantiate(request.instanceId);
                    break;
                case MessageType.Primitive:
                    PrimitiveData primitive = new PrimitiveMessage(data).data;
                    ReceiveValues(primitive);
                    break;

                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        void SendDirtyValues()
        {
            if (_networkClient == null)
                return;

            foreach (int[] route in _reflection.DirtyRoutes)
            {
                _networkClient.SendToServer(_reflection.GetMessage(route).Serialize());
            }
        }

        private void ReceiveValues(PrimitiveData data)
        {
            if (_reflection != null)
                _reflection.SetDataAt(data.route, data.obj, true);
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            clientId = response.ServerHandshakeData.id;
            Debug.Log($"Client Id {clientId}");
        }

        private void HandlePrivateHsResponseData(PrivateServerHsResponse response)
        {
        }

        private void HandleQuit()
        {
            //_clientFactory.DeInstantiateAll();
            _networkClient.EndClient();
        }

        private void HandleAbruptDisconnection()
        {
            _clientFactory.DeInstantiateAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleDisconnectedUser(int id)
        {
            throw new NotImplementedException();
        }

        public void SendInstantiateRequest(GameObject prefab, Matrix4x4 trs, short instanceColor)
        {
            if (!prefabsData.prefabToHash.ContainsKey(prefab))
            {
                Debug.Log("Invalid Prefab");
                return;
            }

            InstanceData instanceData = new InstanceData
            {
                originalClientID = clientId,
                prefabHash = prefabsData.prefabToHash[prefab],
                instanceID = -1,
                trs = ByteFormat.Get4X4Bytes(trs),
                color = instanceColor
            };

            _networkClient.SendToServer(new InstantiateRequest(instanceData).Serialize());
        }

        public void SendDeInstantiateRequest(GameObject gameObject)
        {
            _clientFactory.TryGetInstanceId(gameObject, out int instanceId, out int originalClientId);
            _networkClient.SendToServer(new DeInstantiateRequest(instanceId).Serialize());
        }

        public void SendIntegrityCheck(InstanceData instanceData)
        {
            _networkClient.SendToServer(new InstanceIntegrityCheck(instanceData).Serialize());
        }

        public string GetUsername(int id)
        {
            return _networkClient.GetUsername(id);
        }

        private void HandleError(string error)
        {
            errorPanel.gameObject.SetActive(true);
            errorPanel.SetText(error);
        }
    }
}