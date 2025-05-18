using System;
using System.Net;
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

    [SerializeField] private GameObject pingTextObject;

    [SerializeField] private GameObject chatScreen;

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

        pingTextObject.SetActive(true);
        ClientManager.Instance.networkClient.onDisconnection += Disconnect;

        SwitchToFps();
    }

    void SwitchToChatScreen()
    {
        this.gameObject.SetActive(false);
    }

    void SwitchToFps()
    {
        this.gameObject.SetActive(false);
        chatScreen.SetActive(true);
    }

    public void Disconnect()
    {
        this.gameObject.SetActive(true);
        if (ClientManager.Instance.networkClient != null)
            ClientManager.Instance.networkClient.onDisconnection -= Disconnect;
    }
}