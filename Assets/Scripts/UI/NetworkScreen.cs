using System;
using System.Collections.Generic;
using System.Net;
using Input;
using Network;
using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public InputField addressInputField;
    public InputField usernameInputField;
    public InputField rateInputField;
    public Dropdown colorDropdown;

    [SerializeField] private GameObject ping;

    [SerializeField] private GameObject chatScreen;
    [SerializeField] private GameObject mainCamera;

    [SerializeField] private List<GameObject> gameOverPanels = new List<GameObject>();

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        string username = usernameInputField.text;
        int elo = Convert.ToInt32(rateInputField.text);
        short color = (short)colorDropdown.value;
        Debug.Log($"Color Number {color}");

        Debug.Log(ClientManager.Instance.networkClient.defaultPort);
        ClientManager.Instance.networkClient.StartClient(ipAddress, ClientManager.Instance.networkClient.defaultPort, username, elo, color);

        ClientManager.Instance.networkClient.onDisconnection += Disconnect;

        InputReader.Instance.onPingScreen += OnPingScreen;
        SwitchToFps();
    }

    private void OnPingScreen(bool shouldEnable)
    {
        ping.SetActive(shouldEnable);
    }

    void SwitchToFps()
    {
        this.gameObject.SetActive(false);
        chatScreen.SetActive(true);
    }

    public void Disconnect()
    {
        gameObject.SetActive(true);
        foreach (var panelObject in gameOverPanels)
        {
            panelObject.SetActive(false);
        }

        if (Camera.main == null)
            GameObject.Instantiate(mainCamera);

        InputReader.Instance.onPingScreen -= OnPingScreen;
        if (ClientManager.Instance.networkClient != null)
            ClientManager.Instance.networkClient.onDisconnection -= Disconnect;
    }
}