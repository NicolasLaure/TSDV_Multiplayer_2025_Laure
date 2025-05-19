namespace Network;

public struct SavedClient
{
    public string username;
    public int elo = 3000;
    public int playedGames;
    public int wonGames;
    public bool isBanned = false;

    public SavedClient(string username)
    {
        this.username = username;
        this.elo = 3000;
        this.isBanned = false;
        playedGames = 0;
        wonGames = 0;
    }
}