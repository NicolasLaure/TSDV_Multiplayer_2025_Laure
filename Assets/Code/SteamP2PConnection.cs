using UnityEngine;
using Steamworks;
using System.Text;

public class SteamP2PConnection : MonoBehaviour
{
    private const int Channel = 0;

    void Start()
    {
        if (!Packsize.Test() || !DllCheck.Test())
        {
            Debug.LogError("Steamworks.NET: Incompatible SteamAPI DLL.");
            return;
        }

		if (!SteamAPI.IsSteamRunning())
		{
            Debug.LogError("Steam is not running.");
            return;
		}

        if (!SteamAPI.Init())
        {
            Debug.LogError("SteamAPI.Init() failed.");
            return;
        }

        Debug.Log("SteamAPI initialized. SteamID: " + SteamUser.GetSteamID());
        Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    }

    void Update()
    {
        SteamAPI.RunCallbacks();
        CheckIncomingMessages();
    }

    void OnDestroy()
    {
        SteamAPI.Shutdown();
        Debug.Log("SteamAPI shut down.");
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t request)
    {
        Debug.Log("Received P2P session request from: " + request.m_steamIDRemote);
        SteamNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
    }

    private void CheckIncomingMessages()
    {
        while (SteamNetworking.IsP2PPacketAvailable(out uint msgSize, Channel))
        {
            byte[] buffer = new byte[msgSize];
            if (SteamNetworking.ReadP2PPacket(buffer, msgSize, out uint bytesRead, out CSteamID senderId, Channel))
            {
                string message = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                Debug.Log($"Message received from {senderId}: {message}");
            }
        }
    }

    public void SendMessageToUser(CSteamID userId, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        bool sent = SteamNetworking.SendP2PPacket(userId, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable, Channel);
        Debug.Log(sent ? "Message sent." : "Failed to send message.");
    }
}
