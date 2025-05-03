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

        private List<Vector3> _positions = new List<Vector3>();

        protected override void Initialize()
        {
            if (ServerManager.Instance == null)
                return;

            ServerManager.Instance.onNewClient += HandleNewClient;
            ServerManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
        {
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, 0);
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
            _positions[index] = pos;
        }

        private void HandleNewClient(int id)
        {
            if (id > _positions.Count - 1)
                _positions.Add(new Vector3(horizontalOffset * this._positions.Count, 0, 0));

            PublicHandshakeResponse hsResponse = new PublicHandshakeResponse(id, _positions.Count, ServerManager.Instance.Seed, _positions);
            ServerManager.Instance.SendToClient(hsResponse.Serialize(), id);
        }
    }
}