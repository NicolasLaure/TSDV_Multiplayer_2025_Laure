using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using Network;
using UnityEngine.Serialization;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;

    [SerializeField] private GameObject pingTextObject;
    [SerializeField] private GameObject movingCubes;

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
        int port = System.Convert.ToInt32(portInputField.text);

        Instantiate(clientPrefab);
        ClientManager.Instance.StartClient(ipAddress, port);
        movingCubes.SetActive(true);

        pingTextObject.SetActive(true);

        if (ChatScreen.Instance != null)
            SwitchToChatScreen();
        else
            SwitchToCubes();
    }

    void OnStartServerBtnClick()
    {
        int port = System.Convert.ToInt32(portInputField.text);

        Instantiate(serverPrefab);
        ServerManager.Instance.StartServer(port);
        movingCubes.SetActive(true);

        if (ChatScreen.Instance != null)
            SwitchToChatScreen();
        else
        {
            SwitchToCubes();
            MovingCubes.Instance.HandleServerStart();
        }
    }

    void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    void SwitchToCubes()
    {
        MovingCubes.Instance.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }
}