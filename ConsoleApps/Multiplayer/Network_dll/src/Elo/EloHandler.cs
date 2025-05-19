using System;
using Network.FileManagement;
using Network.Utilities;

namespace Network.Elo;

public class EloHandler
{
    private SavedClientHandler _saveClientHandler;

    public EloHandler(SavedClientHandler saveClientHandler)
    {
        _saveClientHandler = saveClientHandler;
    }

    public void EloCalculation(string winnerUsername, string loserUsername)
    {
        SavedClient winnerData = _saveClientHandler.GetClientData(winnerUsername);
        winnerData.playedGames++;
        winnerData.wonGames++;


        SavedClient loserData = _saveClientHandler.GetClientData(loserUsername);
        loserData.playedGames++;

        double winnerProbability = Probability(winnerData.elo, loserData.elo);
        double loserProbability = Probability(loserData.elo, winnerData.elo);

        int winnerModifier = (int)Math.Ceiling(40f * (1 + (float)winnerData.wonGames / winnerData.playedGames));
        int loserModifier = (int)Math.Ceiling(40f * (2f - (float)winnerData.wonGames / winnerData.playedGames));

        Logger.Log($"{winnerUsername} previous Elo: {winnerData.elo}, {loserUsername} previous Elo: {loserData.elo}");
        winnerData.elo = (int)Math.Floor(winnerData.elo + winnerModifier * (1 - winnerProbability));
        loserData.elo = (int)Math.Floor(loserData.elo + loserModifier * -loserProbability);

        Logger.Log($"{winnerUsername} new Elo: {winnerData.elo}, {loserUsername} new Elo: {loserData.elo}");
        _saveClientHandler.SaveClient(winnerData);
        _saveClientHandler.SaveClient(loserData);
    }

    private double Probability(int rating1, int rating2)
    {
        return 1.0 / (1 + Math.Pow(10, (rating1 - rating2) / 400.0));
    }
}