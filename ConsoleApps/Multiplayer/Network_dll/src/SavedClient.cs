namespace Network;

public struct SavedClient
{
    public string username;
    public int elo = 3000;
    public bool isBanned = false;

    public SavedClient(string username)
    {
        this.username = username;
        this.elo = 3000;
        this.isBanned = false;
    }
}