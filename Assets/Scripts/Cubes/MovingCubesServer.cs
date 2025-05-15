// using System;
// using System.Collections.Generic;
// using System.Net;
// using Network;
// using Network.Enums;
// using Network.Messages;
// using Network.Messages.Server;
// using UnityEngine;
//
// namespace Cubes
// {
//     public class MovingCubesServer : MonoBehaviourSingleton<MovingCubesServer>
//     {
//         [SerializeField] private float horizontalOffset;
//
//         private List<Cube> _cubes = new List<Cube>();
//
//         private void OnEnable()
//         {
//             NonAuthoritativeServer.onServerStart += Initialize;
//         }
//
//         protected override void Initialize()
//         {
//             if (NonAuthoritativeServer.Instance == null)
//                 return;
//
//             NonAuthoritativeServer.Instance.onNewClient += HandleNewClient;
//             NonAuthoritativeServer.Instance.OnReceiveEvent += OnReceiveDataEvent;
//             NonAuthoritativeServer.Instance.onClientRemoved += RemoveClient;
//         }
//
//         void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
//         {
//             MessageType messageType = (MessageType)BitConverter.ToInt16(data, MessageOffsets.MessageTypeIndex);
//             switch (messageType)
//             {
//                 case MessageType.Position:
//                     ReceiveCubePos(data);
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//         }
//
//         private void ReceiveCubePos(byte[] data)
//         {
//             Position posMessage = new Position(data);
//             Vector3 pos = posMessage.pos;
//             int index = posMessage.instanceID;
//             _cubes[index].position = pos;
//         }
//
//         private void HandleNewClient(int id)
//         {
//             if (id > _cubes.Count - 1)
//                 _cubes.Add(new Cube(new Vector3(horizontalOffset * _cubes.Count, 0, 0)));
//
//             ServerHsResponse hsResponse = new ServerHsResponse(id, _cubes.Count, NonAuthoritativeServer.Instance.Seed, _cubes);
//             NonAuthoritativeServer.Instance.SendToClient(hsResponse.Serialize(), id);
//         }
//
//         private void RemoveClient(int id)
//         {
//             _cubes[id].isActive = false;
//         }
//     }
// }