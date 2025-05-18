using Cubes;
using Network;
using Network_dll.Messages.Data;
using TMPro;
using UnityEngine;

namespace UI
{
    public class PingText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void OnEnable()
        {
            ClientManager.Instance.networkClient.onReceiveAllPings += UpdatePing;
        }

        private void OnDisable()
        {
            ClientManager.Instance.networkClient.onReceiveAllPings -= UpdatePing;
        }

        private void UpdatePing(ClientPing[] pings)
        {
            text.text = "";
            for (int i = 0; i < pings.Length; i++)
            {
                string username = FpsClient.Instance.GetUsername(pings[i].id);
                text.text += $"{username}: {pings[i].ms}ms" + System.Environment.NewLine;
            }
        }
    }
}