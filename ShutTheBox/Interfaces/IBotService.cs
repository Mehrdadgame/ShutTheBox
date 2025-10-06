using ShutTheBox.DTOs;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Models;

namespace ShutTheTwelve.Backend.Interfaces
{
    public interface IBotService
    {
        Task<List<int>> GetBotMove(List<int> board, int die1, int die2);
        bool ShouldUseCard(string cardType, GameStateDto gameState);
    }
}