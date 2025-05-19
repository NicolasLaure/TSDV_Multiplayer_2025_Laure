using System;
using System.Collections.Generic;
using System.Net;
using Network;
using Network.Enums;
using Network.Factory;
using Network.Messages;
using UnityEditor;
using UnityEngine;
using MessageType = Network.Enums.MessageType;

namespace FPS.AuthServer
{
    public class FpsServer : MonoBehaviourSingleton<FpsServer>
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerProperties playerProperties;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private HashHandler prefabsData;
        [SerializeField] private ColorHandler colorHandler;
        public Action<EntityToUpdate> onEntityUpdated;

        private AuthoritativeServer _networkServer;
        private AuthServerFactory _serverFactory;

        private readonly Dictionary<int, PlayerController> idToPlayerController = new Dictionary<int, PlayerController>();

        private void Start()
        {
            _networkServer = ServerMono.Instance.networkServer;
            _serverFactory = ServerMono.Instance.serverFactory;

            _networkServer.OnReceiveEvent += OnReceiveDataEvent;
        }

        void OnReceiveDataEvent(byte[] data, IPEndPoint ip)
        {
            int receivedClientId = _networkServer.GetReceivedClientId(ip);
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
                        InputHandler.Instance.idToMoveActions[receivedClientId]?.Invoke(axisInput.axis);
                    else
                        InputHandler.Instance.idToLookActions[receivedClientId]?.Invoke(axisInput.axis);

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

        private void HandlePrivateHandshake(PrivateHandshake privateHandshake)
        {
            Transform spawnPos = spawnPoints[privateHandshake.clientId];
            InstanceData instanceData;
            instanceData.instanceID = 0;
            instanceData.originalClientID = privateHandshake.clientId;
            instanceData.color = privateHandshake.color;
            instanceData.prefabHash = prefabsData.prefabToHash[playerPrefab];
            instanceData.trs = ByteFormat.Get4X4Bytes(spawnPos.localToWorldMatrix);
            GameObject newPlayer = _serverFactory.Instantiate(instanceData);
            newPlayer.GetComponent<AuthPlayerController>().id = privateHandshake.clientId;
        }

        void OnEntityUpdate(EntityToUpdate entityToToUpdate, int clientId)
        {
            if (_networkServer != null)
            {
                SendEntityPosition(entityToToUpdate.gameObject, entityToToUpdate.trs, clientId);
            }
        }

        private void SendEntityPosition(GameObject entity, Matrix4x4 trs, int clientId)
        {
            if (!_serverFactory.TryGetInstanceId(entity, out int instanceId, out int originalClientId) || originalClientId != clientId)
                return;

            Position entityPosition = new Position(trs, instanceId, clientId);
            _networkServer.Broadcast(entityPosition.Serialize());
        }
    }
}