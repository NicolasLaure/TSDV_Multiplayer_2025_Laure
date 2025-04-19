using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public struct Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public float ping;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        ping = 0;
    }
}

public class NetworkManager : MonoBehaviourSingleton<NetworkManager>, IReceiveData
{
    public IPAddress ipAddress { get; private set; }

    public int port { get; private set; }

    public bool isServer { get; private set; }

    public int TimeOut = 30;
    public short ping = 0;
    public Action<byte[], IPEndPoint> OnReceiveEvent;

    private UdpConnection connection;

    private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
    private readonly Dictionary<IPEndPoint, int> ipToId = new Dictionary<IPEndPoint, int>();
    private readonly Dictionary<int, float> idPingTime = new Dictionary<int, float>();

    int clientId = 0; // This id should be generated during first handshake

    public Action<int> onNewClient;
    public Action<short> onPingUpdated;

    public void StartServer(int port)
    {
        isServer = true;
        this.port = port;
        connection = new UdpConnection(port, this);
        clientId++;
    }

    public void StartClient(IPAddress ip, int port)
    {
        isServer = false;

        this.port = port;
        this.ipAddress = ip;

        connection = new UdpConnection(ip, port, this);
        HandshakeData handshakeData;
        handshakeData.ip = 0;
        SendToServer(new Handshake(handshakeData).Serialize());
    }

    void AddClient(IPEndPoint ip)
    {
        if (!ipToId.ContainsKey(ip))
        {
            int id = clientId;
            ipToId[ip] = clientId;
            Debug.Log("Adding client: " + ip.Address + " ID: " + id);
            clients.Add(clientId, new Client(ip, clientId, Time.realtimeSinceStartup));
            idPingTime[clientId] = Time.time;
            SendToClient(new Ping(0).Serialize(), clientId);

            if (onNewClient != null && isServer)
                onNewClient.Invoke(clientId);
            clientId++;
        }
    }

    void RemoveClient(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
        {
            Debug.Log("Removing client: " + ip.Address);
            clients.Remove(ipToId[ip]);
        }
    }

    public int GetIpId(IPEndPoint ip)
    {
        if (ipToId.ContainsKey(ip))
            return ipToId[ip];

        return -1;
    }

    public void OnReceiveData(byte[] data, IPEndPoint ip)
    {
        int receivedClientId = -1;
        if (ipToId.ContainsKey(ip))
            receivedClientId = ipToId[ip];

        MessageType messageType = (MessageType)BitConverter.ToInt16(data, 0);
        switch (messageType)
        {
            case MessageType.HandShake:
                if (!ipToId.ContainsKey(ip))
                    AddClient(ip);
                break;
            case MessageType.Acknowledge:
                break;
            case MessageType.DisAcknowledge:
                break;
            case MessageType.Disconnect:
                break;
            case MessageType.Error:
                break;
            case MessageType.Ping:
                if (isServer)
                {
                    short ms = (short)Mathf.FloorToInt((Time.time - idPingTime[receivedClientId]) * 1000);
                    idPingTime[receivedClientId] = Time.time;
                    SendToClient(new Ping(ms).Serialize(), receivedClientId);
                }
                else
                {
                    ping = new Ping(data).ms;
                    SendToServer(new Ping(0).Serialize());
                    onPingUpdated.Invoke(ping);
                }

                break;

            //Moving Cubes message
            case MessageType.HandShakeResponse:
            case MessageType.Position:
                if (OnReceiveEvent != null)
                    OnReceiveEvent.Invoke(data, ip);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator PingTest(float delay)
    {
        yield return new WaitForSeconds(delay);
        SendToServer(new Ping(0).Serialize());
    }

    public void SendToServer(byte[] data)
    {
        connection.Send(data);
    }

    public void SendToClient(byte[] data, int clientId)
    {
        if (!isServer)
            return;

        Client client = clients[clientId];
        connection.Send(data, client.ipEndPoint);
    }

    public void Broadcast(byte[] data)
    {
        using (var iterator = clients.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                connection.Send(data, iterator.Current.Value.ipEndPoint);
            }
        }
    }

    void Update()
    {
        // Flush the data in main thread
        if (connection != null)
            connection.FlushReceiveData();
    }
}