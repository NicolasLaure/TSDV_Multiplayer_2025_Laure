using UnityEngine;
using Steamworks;

public class SteamP2PSendMessage : MonoBehaviour
{
    public SteamP2PConnection steamNet;
    public string message = "Patata";
    public string targetSteamId = "TARGET STEAM ID";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CSteamID targetId = new CSteamID(ulong.Parse(targetSteamId));
            steamNet.SendMessageToUser(targetId, message);
        }
    }
}
