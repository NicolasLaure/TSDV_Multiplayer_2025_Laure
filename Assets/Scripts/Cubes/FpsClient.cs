using System;
using System.Collections.Generic;
using System.Net;
using FPS;
using Input;
using Messages.ClientMessages;
using Network;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using UnityEngine;
using UnityEngine.Events;

namespace Cubes
{
    public class FpsClient : MonoBehaviourSingleton<FpsClient>
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerProperties playerProperties;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private HashHandler prefabsData;
        public UnityEvent<Matrix4x4> onPlayerUpdated;
        public UnityEvent<bool> onCrouch;
        private List<GameObject> players = new List<GameObject>();
        private int clientId = -1;

        private int positionMessageId = 0;

        private NetworkClient _networkClient;
        private ClientFactory _clientFactory;

        public static int instantiatedID = -1;

        private void Start()
        {
            prefabsData.Initialize();
            _clientFactory = new ClientFactory(prefabsData);
            onPlayerUpdated.AddListener(OnCubeUpdate);
            onCrouch.AddListener(OnCrouch);

            _networkClient = ClientManager.Instance.networkClient;

            _networkClient.OnReceiveEvent += OnReceiveDataEvent;
            _networkClient.onClientDisconnect += RemoveCube;
            _networkClient.onDisconnection += HandleAbruptDisconnection;

            InputReader.Instance.onQuit += HandleQuit;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
        {
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            switch (messageType)
            {
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponseData(new ServerHsResponse(data));
                    break;
                case MessageType.Position:
                    ReceiveCubePos(data);
                    break;
                case MessageType.Crouch:
                    ReceiveCrouch(data);
                    break;
                case MessageType.InstantiateRequest:
                    _clientFactory.Instantiate(new InstantiateRequest(data).instanceData);
                    break;
                case MessageType.DeInstantiateRequest:
                    DeInstantiateRequest request = new DeInstantiateRequest(data);
                    _clientFactory.DeInstantiate(request.instanceId);
                    break;
                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnCubeUpdate(Matrix4x4 playerTrs)
        {
            if (_networkClient != null)
            {
                SendCubePosition(playerTrs);
            }
        }

        void OnCrouch(bool isCrouching)
        {
            if (_networkClient != null)
            {
                _networkClient.SendToServer(new Crouch(isCrouching, clientId).Serialize());
            }
        }

        private void SendCubePosition(Matrix4x4 playerTrs)
        {
            Position cubePosition = new Position(playerTrs, clientId);
            cubePosition.clientId = clientId;
            _networkClient.SendToServer(cubePosition.Serialize());
            positionMessageId++;
        }

        private void ReceiveCubePos(byte[] data)
        {
            Position posMessage = new Position(data);
            Matrix4x4 trs = posMessage.trs;
            int index = posMessage.instanceID;
            if (index == clientId)
                return;

            while (index >= players.Count)
            {
                players.Add(Instantiate(playerPrefab));
            }

            if (!players[index].activeInHierarchy)
                players[index].SetActive(true);

            players[index].transform.position = trs.GetPosition();
            players[index].transform.rotation = trs.rotation;
        }

        private void ReceiveCrouch(byte[] data)
        {
            Crouch crouch = new Crouch(data);

            if (crouch.clientId > players.Count)
                return;

            if (crouch.isCrouching)
            {
                Debug.Log($"Player[{crouch.clientId}] position Y: {playerProperties.crouchYPosition} Size Y: {playerProperties.crouchSize}");
                players[crouch.clientId].GetComponent<TransformHandler>().SetHeight(playerProperties.crouchSize, playerProperties.crouchYPosition);
            }
            else
            {
                Debug.Log("Player wasn't crouching");
                players[crouch.clientId].GetComponent<TransformHandler>().SetHeight(playerProperties._defaultSize, playerProperties._defaultYPosition);
            }
        }

        private void RemoveCube(int id)
        {
            players[id].SetActive(false);
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            clientId = _networkClient.Id;
            for (int i = 0; i < response.ServerHandshakeData.count; i++)
            {
                Matrix4x4 trs = ByteFormat.Get4X4FromBytes(response.ServerHandshakeData.players[i], sizeof(bool));
                Vector3 position = trs.GetPosition();
                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
                newPlayer.SetActive(BitConverter.ToBoolean(response.ServerHandshakeData.players[i]));
                players.Add(newPlayer);
            }

            Transform spawnPos = spawnPoints[clientId];
            GameObject player = players[clientId];
            player.transform.position = spawnPos.position;
            player.transform.rotation = spawnPos.rotation;
            PlayerController playerController = player.AddComponent<PlayerController>();
            MouseLook playerLook = player.AddComponent<MouseLook>();

            playerController.playerProperties = playerProperties;
            playerLook.playerProperties = playerProperties;

            Camera.main.transform.parent = players[clientId].transform;
            Camera.main.transform.localPosition = playerProperties.cameraOffset;

            SendCubePosition(players[clientId].transform.localToWorldMatrix);
        }

        private void HandleQuit()
        {
            foreach (GameObject cube in players)
            {
                Destroy(cube);
            }

            players.Clear();
            _networkClient.EndClient();
        }

        private void HandleAbruptDisconnection()
        {
            foreach (GameObject cube in players)
            {
                Destroy(cube);
            }

            players.Clear();
        }

        public void SendInstantiateRequest(GameObject prefab, Matrix4x4 trs, short color)
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
                instanceID = instantiatedID,
                trs = ByteFormat.Get4X4Bytes(trs),
                color = color
            };

            _networkClient.SendToServer(new InstantiateRequest(instanceData).Serialize());
            instantiatedID++;
        }
    }
}