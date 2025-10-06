using ShutTheBox.Models;

namespace ShutTheBox.Interfaces
{
    public interface IScoreModeService
    {
        Task<bool> EndRound(int gameId);
        Task<ScoreRoundResult> CalculateRoundScore(int gameId);
        Task<bool> IsGameFinished(int gameId);
        Task<GameEndResult> GetFinalResult(int gameId);
    }
}
