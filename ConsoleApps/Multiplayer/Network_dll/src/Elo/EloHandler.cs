using Network.FileManagement;

namespace Network.Elo;

public class EloHandler
{
    private SavedClientHandler _fileHandler;

    public EloHandler(SavedClientHandler fileHandler)
    {
        _fileHandler = fileHandler;
    }

    public void EloCalculation(int winnerElo, string winnerUsername, int loserElo, string loserUsername)
    {
    }
}