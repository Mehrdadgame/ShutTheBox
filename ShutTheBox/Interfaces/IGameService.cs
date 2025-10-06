using ShutTheBox.DTOs;
using ShutTheBox.Models;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Models;

namespace ShutTheTwelve.Backend.Interfaces
{
    public interface IGameService
    {
        Task<GameSession> CreateGame(int player1Id, int? player2Id, string gameMode, bool isAI);
        Task<GameSession?> GetGameById(int gameId);
        Task<DiceRollResult> RollDice(int gameId);
        Task<bool> MakeMove(int gameId, int playerId, List<int> selectedNumbers);
        Task<bool> EndTurn(int gameId);
        Task<GameStateDto> GetGameState(int gameId);
        Task<bool> UseCard(int gameId, int playerId, string cardType, int? targetDie = null);
        bool ValidateMove(List<int> board, List<int> selection, int die1, int die2);
        bool HasPossibleMoves(List<int> board, int die1, int die2);
    }
}