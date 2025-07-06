using System;
using System.Net;
using Input;
using Network;
using Network_dll.Messages.ClientMessages;
using Network_dll.Messages.Data;
using Network.Enums;
using Network.Messages;
using Network.Messages.Server;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidTerm2
{
    public class CastlesClient : MonoBehaviourSingleton<CastlesClient>
    {
        [SerializeField] private ErrorMessagePanel errorPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [SerializeField] private GameObject castlesGameGO;

        public int clientId = -1;

        public ReflectiveClient<CastlesModel> networkClient;
        private CastlesProgram castlesProgram;

        private void Start()
        {
            networkClient = ClientManager.Instance.networkClient;

            networkClient.onClientStart += OnClientStarted;
            networkClient.OnReceiveEvent += OnReceiveDataEvent;
            networkClient.onDisconnection += HandleAbruptDisconnection;
            networkClient.onClientDisconnect += HandleDisconnectedUser;
            networkClient.onError += HandleError;

            InputReader.Instance.onQuit += HandleQuit;
        }

        private void OnDestroy()
        {
            HandleQuit();
            if (Instance == this)
                Instance = null;
        }

        private void OnClientStarted()
        {
            if (networkClient.port != networkClient.defaultPort)
            {
                GameObject castles = Instantiate(castlesGameGO);
                castlesProgram = castles.GetComponent<CastlesProgram>();
                castlesProgram.Initialize(networkClient);
            }
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
                    InstantiateRequest instanceRequest = new InstantiateRequest(data);
                    InstanceData instancedData = networkClient.factory.Instantiate(instanceRequest.instanceData);
                    networkClient.SendIntegrityCheck(instancedData);
                    break;
                case MessageType.DeInstantiateRequest:
                    DeInstantiateRequest request = new DeInstantiateRequest(data);
                    networkClient.factory.DeInstantiate(request.instanceId);
                    break;
                case MessageType.Primitive:
                    PrimitiveData primitive = new PrimitiveMessage(data).data;
                    castlesProgram.reflection.ReceiveValues(primitive);
                    break;
                case MessageType.Rpc:
                    RPCMessage rpcMessage = new RPCMessage(data);
                    if (rpcMessage.clientId != networkClient.Id)
                    {
                        RpcData rpc = rpcMessage.data;
                        castlesProgram.reflection.rpcHooker.ReceiveRPCMessage(rpc);
                    }

                    break;

                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            clientId = response.ServerHandshakeData.id;
            Debug.Log($"Client Id {clientId}");
        }

        private void HandlePrivateHsResponseData(PrivateServerHsResponse response)
        {
            networkClient.factory.InstantiateMultiple(response.objectsToInstantiate);
        }

        private void HandleQuit()
        {
            //_clientFactory.DeInstantiateAll();
            networkClient.EndClient();
        }

        private void HandleAbruptDisconnection()
        {
            //_clientFactory.DeInstantiateAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleDisconnectedUser(int id)
        {
            throw new NotImplementedException();
        }

        public string GetUsername(int id)
        {
            return networkClient.GetUsername(id);
        }

        private void HandleError(string error)
        {
            errorPanel.gameObject.SetActive(true);
            errorPanel.SetText(error);
        }
    }
}