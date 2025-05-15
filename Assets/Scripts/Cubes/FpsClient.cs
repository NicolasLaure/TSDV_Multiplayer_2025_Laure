using System;
using System.Collections.Generic;
using System.Net;
using Input;
using Network;
using Network.Enums;
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
        public UnityEvent<Matrix4x4> onPlayerUpdated;
        private List<GameObject> players = new List<GameObject>();
        private int instanceID = -1;

        private int positionMessageId = 0;

        private NetworkClient _networkClient;

        private void Start()
        {
            onPlayerUpdated.AddListener(OnCubeUpdate);

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

        private void SendCubePosition(Matrix4x4 playerTrs)
        {
            Position cubePosition = new Position(playerTrs, instanceID);
            cubePosition.clientId = instanceID;
            _networkClient.SendToServer(cubePosition.Serialize());
            positionMessageId++;
        }

        private void ReceiveCubePos(byte[] data)
        {
            Position posMessage = new Position(data);
            Matrix4x4 trs = posMessage.trs;
            int index = posMessage.instanceID;
            while (index >= players.Count)
            {
                players.Add(Instantiate(playerPrefab));
            }

            if (!players[index].activeInHierarchy)
                players[index].SetActive(true);

            players[index].transform.position = trs.GetPosition();
        }

        private void RemoveCube(int id)
        {
            players[id].SetActive(false);
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            instanceID = _networkClient.Id;
            for (int i = 0; i < response.ServerHandshakeData.count; i++)
            {
                Matrix4x4 trs = ByteFormat.Get4X4FromBytes(response.ServerHandshakeData.players[i], sizeof(bool));
                Vector3 position = trs.GetPosition();
                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
                newPlayer.SetActive(BitConverter.ToBoolean(response.ServerHandshakeData.players[i]));
                players.Add(newPlayer);
            }

            Transform spawnPos = spawnPoints[instanceID];
            GameObject player = players[instanceID];
            player.transform.position = spawnPos.position;
            player.transform.rotation = spawnPos.rotation;
            PlayerController playerController = player.AddComponent<PlayerController>();
            MouseLook playerLook = player.AddComponent<MouseLook>();

            playerController.speed = playerProperties.speed;
            playerLook.mouseSensitivity = playerProperties.mouseSensitivity;
            playerLook.maxVerticalRotation = playerProperties.maxVerticalRotation;

            Camera.main.transform.parent = players[instanceID].transform;
            Camera.main.transform.localPosition = playerProperties.cameraOffset;

            SendCubePosition(players[instanceID].transform.localToWorldMatrix);
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
    }
}