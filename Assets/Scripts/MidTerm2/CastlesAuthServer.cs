using System;
using System.Net;
using CustomMath;
using FPS.AuthServer;
using Network;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using Network.Messages.Server;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesAuthServer : MonoBehaviourSingleton<CastlesAuthServer>
    {
        [SerializeField] private ColorHandler color;
        [SerializeField] private HashHandler hashHandler;

        public ReflectiveAuthoritativeServer<CastlesModel> _server;
        public ReflectionHandler<CastlesModel> _reflectionHandler;
        //        private AuthServerFactory _serverFactory;

        private void Start()
        {
            _server = new ReflectiveAuthoritativeServer<CastlesModel>();

            string[] args = Environment.GetCommandLineArgs();
            _server.Start(int.Parse(args[1]));
            _server.onNewClient += HandleHandshake;
            _server.OnReceiveEvent += OnReceiveDataEvent;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = _server.GetReceivedClientId(ip);
            MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
            switch (messageType)
            {
                case MessageType.PrivateHandshake:
                    PrivateHandshake privateHandshake = new PrivateHandshake(data);
                    HandlePrivateHandshake(privateHandshake);
                    break;
                case MessageType.AxisInput:
                    AxisInput axisInput = new AxisInput(data);
                    if (axisInput.axisType == AxisType.Move)
                        InputHandler.Instance.idToMoveActions[receivedClientId]?.Invoke(new Vec3(axisInput.axis));
                    else
                        InputHandler.Instance.idToLookActions[receivedClientId]?.Invoke(new Vec3(axisInput.axis));

                    break;
                case MessageType.ActionInput:
                    ActionInput actionInput = new ActionInput(data);
                    if (actionInput.actionType == (short)ActionType.Crouch)
                        InputHandler.Instance.idToCrouchActions[receivedClientId]?.Invoke();
                    else
                        InputHandler.Instance.idToShootActions[receivedClientId]?.Invoke();
                    break;

                default:
                    Debug.Log($"MessageType = {(int)messageType}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleHandshake(int clientId)
        {
            ServerHsResponse hsResponse = new ServerHsResponse(clientId, _server.Seed);
            _server.SendToClient(hsResponse.Serialize(), clientId);
        }

        private void HandlePrivateHandshake(PrivateHandshake privateHandshake)
        {
        }

        // void OnEntityUpdate((EntityToUpdate entityToToUpdate, int clientId) entityAndId)
        // {
        //     if (_server != null)
        //     {
        //         SendEntityPosition(entityAndId.entityToToUpdate.gameObject, entityAndId.entityToToUpdate.trs, entityAndId.clientId);
        //     }
        // }
        //
        // private void SendEntityPosition(GameObject entity, Matrix4x4 trs, int clientId)
        // {
        //     if (!_serverFactory.TryGetInstanceId(entity, out int instanceId, out int originalClientId) || originalClientId != clientId)
        //         return;
        //
        //     Position entityPosition = new Position(trs, instanceId, clientId);
        //     _server.Broadcast(entityPosition.Serialize());
        // }

        public void Instantiate(GameObject prefab, Matrix4x4 trs, short instanceColor, int clientId)
        {
            if (!hashHandler.prefabToHash.ContainsKey(prefab))
            {
                Debug.Log("Invalid Prefab");
                return;
            }

            InstanceData instanceData = new InstanceData
            {
                originalClientID = clientId,
                prefabHash = hashHandler.prefabToHash[prefab],
                instanceID = -1,
                trs = ByteFormat.Get4X4Bytes(trs),
                color = instanceColor
            };

            //_serverFactory.Instantiate(instanceData);
        }
    }
}