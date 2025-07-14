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
    public Dropdown colorDropdown;

    [SerializeField] private GameObject ping;

    [SerializeField] private GameObject chatScreen;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private InputReader input;
    [SerializeField] private List<GameObject> gameOverPanels = new List<GameObject>();

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        string username = usernameInputField.text;
        short color = (short)colorDropdown.value;

        Debug.Log(ClientManager.Instance.networkClient.defaultPort);
        ClientManager.Instance.networkClient.StartClient(ipAddress, ClientManager.Instance.networkClient.defaultPort, username, color);
        ClientManager.Instance.isServerActive = true;

        ClientManager.Instance.networkClient.onDisconnection += Disconnect;

        input.onPingScreen += OnPingScreen;
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
            Instantiate(mainCamera);

        input.onPingScreen -= OnPingScreen;
        if (ClientManager.Instance.networkClient != null)
            ClientManager.Instance.networkClient.onDisconnection -= Disconnect;
    }
}