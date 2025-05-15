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
    public class MovingCubesClient : MonoBehaviourSingleton<MovingCubesClient>
    {
        public UnityEvent<Vector3> onCubeUpdated;
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private float playerSpeed;
        private List<GameObject> cubes = new List<GameObject>();
        private int instanceID = -1;

        private int positionMessageId = 0;

        private NetworkClient _networkClient;

        protected override void Initialize()
        {
            onCubeUpdated.AddListener(OnCubeUpdate);

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

        void OnCubeUpdate(Vector3 pos)
        {
            if (_networkClient != null)
            {
                SendCubePosition(pos);
            }
        }

        private void SendCubePosition(Vector3 pos)
        {
            _networkClient.SendToServer(new Position(pos, instanceID).Serialize());
            positionMessageId++;
        }

        private void ReceiveCubePos(byte[] data)
        {
            Position posMessage = new Position(data);
            Vector3 pos = posMessage.pos;
            int index = posMessage.instanceID;
            while (index >= cubes.Count)
            {
                cubes.Add(Instantiate(cubePrefab));
            }

            if (!cubes[index].activeInHierarchy)
                cubes[index].SetActive(true);

            cubes[index].transform.position = pos;
        }

        private void RemoveCube(int id)
        {
            cubes[id].SetActive(false);
        }

        private void HandleHandshakeResponseData(ServerHsResponse response)
        {
            instanceID = _networkClient.Id;
            for (int i = 0; i < response.ServerHandshakeData.count; i++)
            {
                Vector3 position = ByteFormat.GetVector3FromBytes(response.ServerHandshakeData.cubes[i], sizeof(bool));
                GameObject newCube = Instantiate(cubePrefab, position, Quaternion.identity);
                newCube.SetActive(BitConverter.ToBoolean(response.ServerHandshakeData.cubes[i]));
                cubes.Add(newCube);
            }

            cubes[instanceID].AddComponent<CubeController>();
            cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;

            SendCubePosition(cubes[instanceID].transform.position);
        }

        private void HandleQuit()
        {
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }

            cubes.Clear();
            _networkClient.EndClient();
        }

        private void HandleAbruptDisconnection()
        {
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }

            cubes.Clear();
        }

        [ContextMenu("OrderTest")]
        public void OrderTest()
        {
            if (_networkClient != null)
                _networkClient.SendToServer(new Position(new Vector3(0, 0, 0), instanceID).Serialize());
        }
    }
}