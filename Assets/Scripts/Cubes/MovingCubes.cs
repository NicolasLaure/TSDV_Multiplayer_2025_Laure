using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
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
        MyNetworkPacket receivedPacket = MyNetworkPacket.GetPacket(data);
        if (receivedPacket == null)
            Debug.Log("Error");
        switch (receivedPacket.type)
        {
            case PacketType.HandShake:
                HandleHandshakeData(receivedPacket.payload);
                break;
            case PacketType.HandShake_OK:
                break;
            case PacketType.Error:
                break;
            case PacketType.Ping:
                break;
            case PacketType.Pong:
                break;
            case PacketType.Message:
                if (NetworkManager.Instance.isServer)
                {
                    NetworkManager.Instance.Broadcast(data);
                }

                Debug.Log($"array length = {receivedPacket.payload.Length}");
                ReceiveCubePos(receivedPacket.payload);
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
        byte[] data = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(index), 0, data, 0, 4);
        Buffer.BlockCopy(GetVector3Bytes(pos), 0, data, 4, 12);

        MyNetworkPacket packet = new MyNetworkPacket(PacketType.Message, Attributes.None, data, Time.time, instanceID);
        NetworkManager.Instance.Broadcast(packet.GetBytes());
    }

    private void SendCubePosition(Vector3 pos)
    {
        byte[] data = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(instanceID), 0, data, 0, 4);
        Buffer.BlockCopy(GetVector3Bytes(pos), 0, data, 4, 12);

        MyNetworkPacket packet = new MyNetworkPacket(PacketType.Message, Attributes.None, data, Time.time, instanceID);
        NetworkManager.Instance.SendToServer(packet.GetBytes());
    }

    private void ReceiveCubePos(byte[] data)
    {
        Debug.Log($"array length = {data.Length}");
        int index = System.BitConverter.ToInt32(data, 0);
        byte[] componentsData = new byte[12];
        Buffer.BlockCopy(data, 4, componentsData, 0, 12);
        while (index >= cubes.Count)
        {
            cubes.Add(GameObject.Instantiate(cubePrefab));
        }

        cubes[index].transform.position = GetVector3FromBytes(componentsData);
    }

    private void HandleNewClient(int id)
    {
        GameObject newCube = GameObject.Instantiate(cubePrefab, new Vector3(horizontalOffset * cubes.Count, 0, 0), Quaternion.identity);
        cubes.Add(newCube);
        byte[] data = new byte[4 + 4 + cubes.Count * 12];
        Buffer.BlockCopy(BitConverter.GetBytes(id), 0, data, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(cubes.Count), 0, data, 4, 4);
        for (int i = 0; i < cubes.Count; i++)
        {
            Buffer.BlockCopy(GetVector3Bytes(cubes[i].transform.position), 0, data, 8 + i * 12, 12);
        }

        MyNetworkPacket packet = new MyNetworkPacket(PacketType.HandShake, Attributes.Important, data, Time.time, id);
        NetworkManager.Instance.SendToClient(packet.GetBytes(), id);
    }

    private void HandleHandshakeData(byte[] data)
    {
        instanceID = System.BitConverter.ToInt32(data, 0);
        int count = System.BitConverter.ToInt32(data, 4);

        for (int i = 0; i < count; i++)
        {
            byte[] componentsBytes = new byte[12];
            Buffer.BlockCopy(data, 8 + 12 * i, componentsBytes, 0, 12);
            GameObject newCube = Instantiate(cubePrefab, GetVector3FromBytes(componentsBytes), Quaternion.identity);
            cubes.Add(newCube);
        }

        cubes[instanceID].AddComponent<CubeController>();
        cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;

        SendCubePosition(cubes[instanceID].transform.position);
    }

    public void HandleServerStart()
    {
        instanceID = 0;
        GameObject newCube = GameObject.Instantiate(cubePrefab, new Vector3(horizontalOffset * cubes.Count, 0, 0), Quaternion.identity);
        cubes.Add(newCube);

        cubes[instanceID].AddComponent<CubeController>();
        cubes[instanceID].GetComponent<CubeController>().Speed = playerSpeed;
    }

    #region Converters

    private byte[] GetVector3Bytes(Vector3 input)
    {
        byte[] vec3Bytes = new byte[12];
        Buffer.BlockCopy(BitConverter.GetBytes(input.x), 0, vec3Bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.y), 0, vec3Bytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.z), 0, vec3Bytes, 8, 4);

        return vec3Bytes;
    }

    private Vector3 GetVector3FromBytes(byte[] bytes)
    {
        List<byte[]> components = new List<byte[]>();
        for (int i = 0; i < 3; i++)
        {
            byte[] componentBytes = new byte[4];
            Buffer.BlockCopy(bytes, i * 4, componentBytes, 0, 4);
            components.Add(componentBytes);
        }

        return new Vector3(System.BitConverter.ToSingle(components[0]), System.BitConverter.ToSingle(components[1]), System.BitConverter.ToSingle(components[2]));
    }

    #endregion
}