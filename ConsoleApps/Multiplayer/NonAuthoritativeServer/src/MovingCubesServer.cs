using System;
using System.Collections.Generic;
using System.Net;
using Network;
using Network.Enums;
using Network.Messages;
using Network.Messages.Server;
using Utils;

namespace Cubes
{
    public class MovingCubesServer
    {
        private List<byte[]> _players = new List<byte[]>();

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
            byte[] pos = posMessage.trs;
            int index = posMessage.instanceID;

            //Buffer.BlockCopy(pos, 0, _players[index], sizeof(bool), pos.Length);
            _players[index] = pos;
        }

        private void HandleNewClient(int id)
        {
            if (id > _players.Count - 1)
            {
                byte[] player = new byte[sizeof(bool) + Constants.MatrixSize];
                Buffer.BlockCopy(BitConverter.GetBytes(true), 0, player, 0, sizeof(bool));
                _players.Add(player);
            }

            ServerHsResponse hsResponse = new ServerHsResponse(id, _players.Count, serverInstance.Seed, _players);
            serverInstance.SendToClient(hsResponse.Serialize(), id);
        }

        private void RemoveClient(int id)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(false), 0, _players[id], 0, sizeof(bool));
        }
    }
}