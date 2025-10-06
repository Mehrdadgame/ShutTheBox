using ShutTheBox.Services;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Models;

namespace ShutTheTwelve.Backend.Interfaces
{
    public interface IMatchmakingService
    {
        Task<MatchmakingResult> FindMatch(int playerId, string gameMode);
        void CancelMatchmaking(int playerId);
        bool IsPlayerInQueue(int playerId);
    }
}