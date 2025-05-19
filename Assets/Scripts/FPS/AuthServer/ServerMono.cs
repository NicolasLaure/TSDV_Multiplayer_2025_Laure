using Network;
using Network.Factory;
using UnityEngine;

public class ServerMono : MonoBehaviourSingleton<ServerMono>
{
    public AuthoritativeServer networkServer;

    public AuthServerFactory serverFactory;
    [SerializeField] private HashHandler prefabsData;
    [SerializeField] private ColorHandler colorHandler;

    protected override void Initialize()
    {
        serverFactory = new AuthServerFactory(prefabsData, colorHandler);
        networkServer = new AuthoritativeServer(serverFactory);
    }

    private void Update()
    {
        networkServer.Update();
    }

    private void OnDestroy()
    {
        networkServer.EndServer();
        if (Instance == this)
            Instance = null;
    }
}