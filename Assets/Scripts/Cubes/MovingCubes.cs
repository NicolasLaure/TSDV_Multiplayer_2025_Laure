using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Network;
using Network.Enums;
using Network.Messages;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;

public class MovingCubes : MonoBehaviourSingleton<MovingCubes>
{
    public UnityEvent<Vector3> onCubeUpdated;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private float playerSpeed;
    [SerializeField] private float horizontalOffset;
    private List<GameObject> cubes = new List<GameObject>();
    private int instanceID = -1;

    private int positionMessageId = 0;

    protected override void Initialize()
    {
        onCubeUpdated.AddListener(OnCubeUpdate);
        if (ServerManager.Instance != null)
        {
            ServerManager.Instance.onNewClient += HandleNewClient;
            ServerManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
        }

        if (ClientManager.Instance)
            ClientManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
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
        if (ServerManager.Instance != null)
        {
            BroadCastCubePosition(pos);
        }

        if (ClientManager.Instance != null)
        {
            SendCubePosition(pos);
        }
    }

    private void BroadCastCubePosition(Vector3 pos)
    {
        NetworkManager<ServerManager>.Instance.Broadcast(new Position(pos, instanceID, positionMessageId).Serialize());
        positionMessageId++;
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

        cubes[index].transform.position = pos;
    }

    private void HandleNewClient(int id)
    {
        GameObject newCube = Instantiate(cubePrefab, new Vector3(horizontalOffset * cubes.Count, 0, 0), Quaternion.identity);
        cubes.Add(newCube);

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < cubes.Count; i++)
        {
            positions.Add(cubes[i].transform.position);
        }

        HandshakeResponse hsResponse = new HandshakeResponse(id, cubes.Count, ServerManager.Instance.Seed, positions);
        ServerManager.Instance.SendToClient(hsResponse.Serialize(), id);
    }

    private void HandleHandshakeResponseData(HandshakeResponse response)
    {
        instanceID = response._handshakeData.id;
        ClientManager.Instance.Seed = response._handshakeData.seed;
        Debug.Log($"Seed: {ClientManager.Instance.Seed}");

        for (int i = 0; i < response._handshakeData.count; i++)
        {
            GameObject newCube = Instantiate(cubePrefab, response._handshakeData.positions[i], Quaternion.identity);
            cubes.Add(newCube);
        }

        cubes[instanceID].AddComponent<CubeController>();
        cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;

        SendCubePosition(cubes[instanceID].transform.position);
    }

    public void HandleServerStart()
    {
        instanceID = 0;
        GameObject newCube = Instantiate(cubePrefab, new Vector3(horizontalOffset * cubes.Count, 0, 0),
        Quaternion.identity);
        cubes.Add(newCube);

        cubes[instanceID].AddComponent<CubeController>();
        cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;
    }

    [ContextMenu("OrderTest")]
    public void OrderTest()
    {
        if (ServerManager.Instance != null)
            NetworkManager<ServerManager>.Instance.Broadcast(new Position(new Vector3(0, 0, 0), instanceID, 0).Serialize());

        if (ClientManager.Instance != null)
            ClientManager.Instance.SendToServer(new Position(new Vector3(0, 0, 0), instanceID, 0).Serialize());
    }
}