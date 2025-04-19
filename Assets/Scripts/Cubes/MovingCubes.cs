using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
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

    protected override void Initialize()
    {
        onCubeUpdated.AddListener(OnCubeUpdate);
        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
        NetworkManager.Instance.onNewClient += HandleNewClient;
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
                if (NetworkManager.Instance.isServer)
                {
                    NetworkManager.Instance.Broadcast(data);
                }

                ReceiveCubePos(data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OnCubeUpdate(Vector3 pos)
    {
        if (NetworkManager.Instance.isServer)
        {
            BroadCastCubePosition(pos, instanceID);
        }
        else
        {
            SendCubePosition(pos);
        }
    }

    private void BroadCastCubePosition(Vector3 pos, int index)
    {
        NetworkManager.Instance.Broadcast(new Position(pos, index).Serialize());
    }

    private void SendCubePosition(Vector3 pos)
    {
        NetworkManager.Instance.SendToServer(new Position(pos, instanceID).Serialize());
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

        HandshakeResponse hsResponse = new HandshakeResponse(id, cubes.Count, positions);
        NetworkManager.Instance.SendToClient(hsResponse.Serialize(), id);
    }

    private void HandleHandshakeResponseData(HandshakeResponse response)
    {
        Debug.Log("SpawningCubes");
        instanceID = response._handshakeData.id;
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
}