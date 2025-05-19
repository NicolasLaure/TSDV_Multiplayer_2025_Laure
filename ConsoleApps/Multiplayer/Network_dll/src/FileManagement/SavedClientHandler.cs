using System.Collections.Generic;
using Newtonsoft.Json;

namespace Network.FileManagement;

public class SavedClientHandler
{
    private FileHandler _fileHandler;

    public SavedClientHandler(string path)
    {
        _fileHandler = new FileHandler(path);
    }

    public SavedClient GetClientData(string username)
    {
        List<SavedClient> savedClients = new List<SavedClient>();
        if (TryGetClientsList(out List<SavedClient> clients))
            savedClients.AddRange(clients);

        foreach (SavedClient savedClient in savedClients)
        {
            if (savedClient.username == username)
                return savedClient;
        }

        SavedClient clientToSave = new SavedClient(username);
        SaveClient(clientToSave);
        return clientToSave;
    }

    public bool TryGetClientsList(out List<SavedClient> savedClients)
    {
        savedClients = JsonConvert.DeserializeObject<List<SavedClient>>(_fileHandler.ReadFile());
        if (savedClients != null)
            return true;
        return false;
    }

    public void SaveClient(SavedClient clientToSave)
    {
        List<SavedClient> savedClients = new List<SavedClient>();
        if (TryGetClientsList(out List<SavedClient> clients))
            savedClients.AddRange(clients);

        for (int i = 0; i < savedClients.Count; i++)
        {
            if (savedClients[i].username == clientToSave.username)
            {
                savedClients[i] = clientToSave;
                _fileHandler.WriteFile(JsonConvert.SerializeObject(savedClients));
                return;
            }
        }

        savedClients.Add(clientToSave);
        _fileHandler.WriteFile(JsonConvert.SerializeObject(savedClients));
    }
}