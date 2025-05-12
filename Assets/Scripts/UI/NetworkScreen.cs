using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using Input;
using Network;
using UnityEngine.Serialization;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField rateInputField;
    public InputField addressInputField;

    [SerializeField] private GameObject pingTextObject;

    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private GameObject serverPrefab;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        int rate = Convert.ToInt32(rateInputField.text);

        Instantiate(clientPrefab);
        ClientManager.Instance.StartClient(ipAddress);

        pingTextObject.SetActive(true);
        ClientManager.Instance.onDisconnection += Disconnect;

        if (ChatScreen.Instance != null)
            SwitchToChatScreen();
        else
            SwitchToCubes();
    }

    void OnStartServerBtnClick()
    {
        int rate = Convert.ToInt32(rateInputField.text);

        Instantiate(serverPrefab);
        ServerManager.Instance.StartServer();

        if (ChatScreen.Instance != null)
            SwitchToChatScreen();
        else
            SwitchToCubes();
    }

    void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    void SwitchToCubes()
    {
        this.gameObject.SetActive(false);
    }

    public void Disconnect()
    {
        this.gameObject.SetActive(true);
        ClientManager.Instance.onDisconnection -= Disconnect;
    }
}