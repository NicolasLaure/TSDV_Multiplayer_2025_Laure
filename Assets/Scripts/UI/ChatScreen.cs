using Cubes;
using Network;
using Network_dll.Messages.ClientMessages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChatScreen : MonoBehaviour
    {
        public Text messages;
        public InputField inputMessage;

        private void Start()
        {
            inputMessage.onEndEdit.AddListener(OnEndEdit);
            ClientManager.Instance.networkClient.onChatMessageReceived += OnReceiveDataEvent;
        }

        void OnReceiveDataEvent(string username, byte[] data)
        {
            Chat chatMessage = new Chat(data);
            messages.text += username + ": " + chatMessage.message + System.Environment.NewLine;
        }

        void OnEndEdit(string str)
        {
            if (inputMessage.text != "")
            {
                Chat message = new Chat(inputMessage.text);
                message.clientId = FpsClient.Instance.clientId;
                ClientManager.Instance.networkClient.SendToServer(message.Serialize());

                inputMessage.ActivateInputField();
                inputMessage.Select();
                inputMessage.text = "";
            }
        }
    }
}