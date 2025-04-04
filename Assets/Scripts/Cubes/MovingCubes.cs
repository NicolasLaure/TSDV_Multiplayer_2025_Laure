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
    [SerializeField] private GameObject cube;

    protected override void Initialize()
    {
        onCubeUpdated.AddListener(OnCubeUpdate);
        cube.SetActive(false);
        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
    }

    private void OnEnable()
    {
        cube.SetActive(true);
    }


    void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(data);
        }

        cube.transform.position = GetVector3FromBytes(data);
    }

    void OnCubeUpdate(Vector3 pos)
    {
        if (NetworkManager.Instance.isServer)
        {
            NetworkManager.Instance.Broadcast(GetVector3Bytes(pos));
        }
        else
        {
            NetworkManager.Instance.SendToServer(GetVector3Bytes(pos));
        }
    }

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
}