using System;
using System.Collections.Generic;
using System.Net;
using Network;
using Network.Enums;
using Network.Messages;
using Network.Messages.Server;

namespace Cubes
{
    public class MovingCubesServer
    {
        private float horizontalOffset = 5f;
        private List<byte[]> _cubes = new List<byte[]>();

        private NonAuthoritativeServer serverInstance;

        public void Start()
        {
            NonAuthoritativeServer.onServerStart += Initialize;
        }

        public MovingCubesServer(NonAuthoritativeServer server)
        {
            serverInstance = server;
        }

        protected void Initialize()
        {
            if (serverInstance == null)
                return;

            serverInstance.onNewClient += HandleNewClient;
            serverInstance.OnReceiveEvent += OnReceiveDataEvent;
            serverInstance.onClientRemoved += RemoveClient;
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
            byte[] pos = posMessage.pos;
            int index = posMessage.instanceID;

            _cubes[index] = pos;
        }

        private void HandleNewClient(int id)
        {
            if (id > _cubes.Count - 1)
            {
                byte[] cube = new byte[13];
                float x = horizontalOffset * _cubes.Count;
                Buffer.BlockCopy(BitConverter.GetBytes(true), 0, cube, 0, sizeof(bool));
                Buffer.BlockCopy(BitConverter.GetBytes(x), 0, cube, sizeof(bool), sizeof(float));
                _cubes.Add(cube);
            }


            ServerHsResponse hsResponse = new ServerHsResponse(id, _cubes.Count, serverInstance.Seed, _cubes);
            serverInstance.SendToClient(hsResponse.Serialize(), id);
        }

        private void RemoveClient(int id)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, _cubes[id], 0, sizeof(bool));
        }
    }
}