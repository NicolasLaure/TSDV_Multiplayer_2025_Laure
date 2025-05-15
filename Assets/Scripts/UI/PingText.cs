using System;
using Network;
using TMPro;
using UnityEngine;

public class PingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private string header = "Ping: ";

    private void OnEnable()
    {
        if (ClientManager.Instance)
        {
            gameObject.SetActive(false);
            return;
        }

        ClientManager.Instance.networkClient.onPingUpdated += UpdatePing;
    }

    private void UpdatePing(short ping)
    {
        text.text = header + ping.ToString();
    }
}