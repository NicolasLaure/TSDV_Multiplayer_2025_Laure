using System;
using TMPro;
using UnityEngine;

public class PingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private string header = "Ping: ";

    private void Start()
    {
        if (NetworkManager.Instance.isServer)
        {
            gameObject.SetActive(false);
            return;
        }

        NetworkManager.Instance.onPingUpdated += UpdatePing;
    }

    private void UpdatePing(short ping)
    {
        text.text = header + ping.ToString();
    }
}