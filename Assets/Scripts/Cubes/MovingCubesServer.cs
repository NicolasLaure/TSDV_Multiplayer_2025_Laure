using System;
using System.Collections.Generic;
using System.Net;
using Network;
using Network.Enums;
using Network.Messages;
using UnityEngine;

namespace Cubes
{
    public class MovingCubesServer : MonoBehaviourSingleton<MovingCubesServer>
    {
        [SerializeField] private float horizontalOffset;

        private List<Cube> _cubes = new List<Cube>();

        protected override void Initialize()
        {
            if (ServerManager.Instance == null)
                return;

            ServerManager.Instance.onNewClient += HandleNewClient;
            ServerManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
            ServerManager.Instance.onClientRemoved += RemoveClient;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
        {
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            switch (messageType)
            {
                case MessageType.Position:
                    ReceiveCubePos(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReceiveCubePos(byte[] data)
        {
            Position posMessage = new Position(data);
            Vector3 pos = posMessage.pos;
            int index = posMessage.instanceID;
            _cubes[index].position = pos;
        }

        private void HandleNewClient(int id)
        {
            if (id > _cubes.Count - 1)
                _cubes.Add(new Cube(new Vector3(horizontalOffset * _cubes.Count, 0, 0)));

            PublicHandshakeResponse hsResponse = new PublicHandshakeResponse(id, _cubes.Count, ServerManager.Instance.Seed, _cubes);
            ServerManager.Instance.SendToClient(hsResponse.Serialize(), id);
        }

        private void RemoveClient(int id)
        {
            _cubes[id].isActive = false;
        }
    }
}