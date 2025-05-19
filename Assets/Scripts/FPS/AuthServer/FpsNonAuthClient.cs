using System;
using System.Collections;
using System.Net;
using Input;
using Messages.ClientMessages;
using Network;
using Network_dll.Messages.ClientMessages;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FPS.AuthServer
{
    public class FpsNonAuthClient : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerProperties playerProperties;
        [SerializeField] private HashHandler prefabsData;
        [SerializeField] private ColorHandler colorHandler;

        [SerializeField] private ErrorMessagePanel errorPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private InputReader _inputReader;
        public int clientId = -1;

        private NetworkClient _networkClient;
        private ClientFactory _clientFactory;

        private bool isGameOver = false;


        private void Start()
        {
            prefabsData.Initialize();
            _clientFactory = new ClientFactory(prefabsData, colorHandler);

            _networkClient = ClientManager.Instance.networkClient;
            _networkClient.onClientStart += Initialize;
        }

        private void Initialize()
        {
            _inputReader.onMove += OnMove;
            _inputReader.onLook += OnLook;
            _inputReader.onCrouch += OnCrouch;
            _inputReader.onShoot += OnShoot;

            _networkClient.OnReceiveEvent += OnReceiveDataEvent;
            _networkClient.onDisconnection += HandleAbruptDisconnection;
            _networkClient.onClientDisconnect += HandleDisconnectedUser;
            _networkClient.onError += HandleError;

            InputReader.Instance.onQuit += HandleQuit;
        }

        private void OnDestroy()
        {
            HandleQuit();
            if (Instance == this)
                Instance = null;
        }

        public Object Instance { get; set; }

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
                case MessageType.Position:
                    Position receivedPosition = new Position(data);
                    ReceiveEntityPos(receivedPosition);
                    break;
                case MessageType.Crouch:
                    Crouch crouch = new Crouch(data);
                    if (crouch.clientId != clientId)
                        ReceiveCrouch(crouch);
                    break;
                case MessageType.InstantiateRequest:
                    _clientFactory.Instantiate(new InstantiateRequest(data).instanceData);
                    break;
                case MessageType.DeInstantiateRequest:
                    DeInstantiateRequest request = new DeInstantiateRequest(data);
                    _clientFactory.DeInstantiate(request.instanceId);
                    break;
                case MessageType.Death:
                    Death death = new Death(data);
                    Debug.Log($"ClientId: {clientId}, deathClientId: {death.deadId}");
                    if (death.deadId != clientId)
                        StartCoroutine(OnGameOver(true));
                    break;

                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnMove(Vector2 movement)
        {
            if (_networkClient != null)
            {
                _networkClient.SendToServer(new AxisInput(movement, AxisType.Move).Serialize());
            }
        }

        void OnLook(Vector2 movement)
        {
            if (_networkClient != null)
            {
                _networkClient.SendToServer(new AxisInput(movement, AxisType.Look).Serialize());
            }
        }

        void OnCrouch()
        {
            if (_networkClient != null)
            {
                _networkClient.SendToServer(new ActionInput(ActionType.Crouch).Serialize());
            }
        }

        void OnShoot()
        {
            if (_networkClient != null)
            {
                _networkClient.SendToServer(new ActionInput(ActionType.Shoot).Serialize());
            }
        }

        private void ReceiveEntityPos(Position posMessage)
        {
            if (posMessage.clientId == clientId)
                return;

            Debug.Log($"ClientId:{clientId}, Pos Received ClientId:{posMessage.clientId}");
            _clientFactory.TryGetOriginalId(posMessage.instanceID, out int originalId);
            Debug.Log($"ClientId:{clientId}, SavedInstanceIdToEntity {originalId}");

            Matrix4x4 trs = posMessage.trs;
            if (_clientFactory.TryGetGameObject(posMessage.instanceID, out GameObject entity))
            {
                entity.transform.position = trs.GetPosition();
                entity.transform.rotation = trs.rotation;
            }
        }

        private void ReceiveCrouch(Crouch crouchData)
        {
            if (!_clientFactory.TryGetGameObject(crouchData.instanceId, out GameObject entity))
                return;

            if (crouchData.isCrouching)
            {
                Debug.Log($"Player[{crouchData.clientId}] position Y: {playerProperties.crouchYPosition} Size Y: {playerProperties.crouchSize}");
                entity.GetComponent<TransformHandler>().SetHeight(playerProperties.crouchSize, playerProperties.crouchYPosition);
            }
            else
            {
                Debug.Log("Player wasn't crouching");
                entity.GetComponent<TransformHandler>().SetHeight(playerProperties._defaultSize, playerProperties._defaultYPosition);
            }
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            clientId = response.ServerHandshakeData.id;
            Debug.Log($"Client Id {clientId}");
        }

        private void HandlePrivateHsResponseData(PrivateServerHsResponse response)
        {
            _clientFactory.InstantiateMultiple(response.objectsToInstantiate);
        }

        private void HandleQuit()
        {
            _clientFactory.DeInstantiateAll();
            _networkClient.EndClient();
        }

        private void HandleAbruptDisconnection()
        {
            _clientFactory.DeInstantiateAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleDisconnectedUser(int id)
        {
            if (id != clientId)
            {
                StartCoroutine(OnGameOver(true));
            }
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

        public IEnumerator OnGameOver(bool hasWon)
        {
            if (isGameOver)
                yield break;

            isGameOver = true;
            if (hasWon)
            {
                winPanel.SetActive(true);
                _networkClient.SendToServer(new Win(_networkClient.username, _networkClient.GetOponentUsername()).Serialize());
            }
            else
                losePanel.SetActive(true);

            _clientFactory.DeInstantiateAll();
            yield return new WaitForSeconds(_networkClient.timeAfterGameOver);
            _networkClient.EndClient();
        }
    }
}