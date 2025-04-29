using System;
using System.Collections.Generic;
using System.Net;
using Input;
using Network;
using Network.Enums;
using Network.Messages;
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

        protected override void Initialize()
        {
            onCubeUpdated.AddListener(OnCubeUpdate);

            if (ClientManager.Instance)
            {
                ClientManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
                ClientManager.Instance.onClientDisconnect += RemoveCube;
            }

            InputReader.Instance.onQuit += HandleQuit;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
        {
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, 0);
            switch (messageType)
            {
                case MessageType.HandShakeResponse:
                    HandleHandshakeResponseData(new HandshakeResponse(data));
                    break;
                case MessageType.Position:
                    ReceiveCubePos(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnCubeUpdate(Vector3 pos)
        {
            if (ClientManager.Instance != null)
            {
                SendCubePosition(pos);
            }
        }

        private void SendCubePosition(Vector3 pos)
        {
            NetworkManager<ClientManager>.Instance.SendToServer(new Position(pos, instanceID, positionMessageId).Serialize());
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

        private void HandleHandshakeResponseData(HandshakeResponse response)
        {
            instanceID = ClientManager.Instance.Id;
            for (int i = 0; i < response._handshakeData.count; i++)
            {
                GameObject newCube = Instantiate(cubePrefab, response._handshakeData.positions[i], Quaternion.identity);
                cubes.Add(newCube);
            }

            cubes[instanceID].AddComponent<CubeController>();
            cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;

            SendCubePosition(cubes[instanceID].transform.position);
        }

        private void HandleQuit()
        {
            ClientManager.Instance.EndClient(instanceID);
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }

            cubes.Clear();
        }

        [ContextMenu("OrderTest")]
        public void OrderTest()
        {
            if (ClientManager.Instance != null)
                ClientManager.Instance.SendToServer(new Position(new Vector3(0, 0, 0), instanceID, 0).Serialize());
        }
    }
}