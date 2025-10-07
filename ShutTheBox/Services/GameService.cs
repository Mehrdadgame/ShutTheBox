using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShutTheTwelveServer.Data;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Models;

namespace ShutTheTwelveServer.Services
{
    public interface IGameService
    {
        Task<GameSession> CreateGameSession(Guid player1Id, Guid? player2Id, string gameMode, bool isAgainstBot);
        Task<DiceRollDTO> RollDice(Guid sessionId, Guid playerId);
        Task<bool> MakeMove(Guid sessionId, Guid playerId, List<int> selectedNumbers);
        Task<GameStateDTO> GetGameState(Guid sessionId, Guid playerId);
        bool ValidateMove(List<int> board, List<int> selection, int dice1, int dice2);
        List<int> GetBotMove(List<int> board, int dice1, int dice2, string difficulty);
        Task EndGame(Guid sessionId, Guid? winnerId);
        Task UpdateScores(Guid winnerId, Guid loserId, int winnerScore, int loserScore);
    }

    public class GameService : IGameService
    {
        private readonly GameDbContext _context;
        private readonly RandomNumberGenerator _rng;

        public GameService(GameDbContext context)
        {
            _context = context;
            _rng = RandomNumberGenerator.Create();
        }

        public async Task<GameSession> CreateGameSession(Guid player1Id, Guid? player2Id, string gameMode, bool isAgainstBot)
        {
            var session = new GameSession
            {
                Id = Guid.NewGuid(),
                Player1Id = player1Id,
                Player2Id = player2Id,
                IsAgainstBot = isAgainstBot,
                GameMode = gameMode,
                State = GameSessionState.InProgress,
                CurrentTurn = RandomBool() ? PlayerTurn.Player1 : PlayerTurn.Player2,
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow
            };

            if (isAgainstBot)
            {
                session.BotDifficulty = "Medium"; // Can be configured
            }

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<DiceRollDTO> RollDice(Guid sessionId, Guid playerId)
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null) throw new Exception("Session not found");

            // Check for active card effects
            var cardUsages = await _context.PowerCardUsages
                .Where(u => u.GameSessionId == sessionId && u.PlayerId == playerId)
                .Include(u => u.PowerCard)
                .ToListAsync();

            var rollResult = new DiceRollDTO
            {
                Dice1 = SecureRandomInt(1, 7),
                Dice2 = SecureRandomInt(1, 7)
            };

            // Apply card effects
            var sabotage = cardUsages.FirstOrDefault(u => u.PowerCard.Type == CardType.Sabotage);
            if (sabotage != null)
            {
                rollResult.IsSabotaged = true;
                rollResult.Dice1 = SecureRandomInt(1, 4);
            }

            var wildDice = cardUsages.FirstOrDefault(u => u.PowerCard.Type == CardType.WildDice);
            if (wildDice != null)
            {
                rollResult.WildDieValue = SecureRandomInt(5, 7);
                if (RandomBool())
                    rollResult.Dice1 = rollResult.WildDieValue.Value;
                else
                    rollResult.Dice2 = rollResult.WildDieValue.Value;
            }

            // Save dice roll to session
            session.LastDice1 = rollResult.Dice1;
            session.LastDice2 = rollResult.Dice2;
            await _context.SaveChangesAsync();

            return rollResult;
        }

        public async Task<bool> MakeMove(Guid sessionId, Guid playerId, List<int> selectedNumbers)
        {
            var session = await _context.GameSessions
                .Include(s => s.Player1)
                .Include(s => s.Player2)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return false;

            // Determine which board to use
            bool isPlayer1 = session.Player1Id == playerId;
            var boardStr = isPlayer1 ? session.Player1Board : session.Player2Board;
            var board = boardStr.Split(',').Select(int.Parse).ToList();

            // Validate move
            if (!ValidateMove(board, selectedNumbers, session.LastDice1, session.LastDice2))
                return false;

            // Remove selected numbers from board
            foreach (var num in selectedNumbers)
            {
                board.Remove(num);
            }

            // Update board
            if (isPlayer1)
                session.Player1Board = string.Join(",", board);
            else
                session.Player2Board = string.Join(",", board);

            // Record move
            var move = new GameMove
            {
                Id = Guid.NewGuid(),
                GameSessionId = sessionId,
                PlayerId = playerId,
                Dice1 = session.LastDice1,
                Dice2 = session.LastDice2,
                NumbersUsed = string.Join(",", selectedNumbers),
                BoardStateBefore = boardStr,
                BoardStateAfter = string.Join(",", board),
                Timestamp = DateTime.UtcNow
            };

            _context.GameMoves.Add(move);

            // Check for winner
            if (board.Count == 0)
            {
                await EndGame(sessionId, playerId);
            }
            else
            {
                // Switch turn
                session.CurrentTurn = session.CurrentTurn == PlayerTurn.Player1
                    ? PlayerTurn.Player2
                    : PlayerTurn.Player1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GameStateDTO> GetGameState(Guid sessionId, Guid playerId)
        {
            var session = await _context.GameSessions
                .Include(s => s.Player1)
                .Include(s => s.Player2)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return null;

            bool isPlayer1 = session.Player1Id == playerId;

            return new GameStateDTO
            {
                SessionId = session.Id,
                GameMode = session.GameMode,
                CurrentTurn = session.CurrentTurn.ToString(),
                PlayerBoard = session.Player1Board.Split(',').Select(int.Parse).ToList(),
                OpponentBoard = session.Player2Board.Split(',').Select(int.Parse).ToList(),
                LastDice1 = session.LastDice1,
                LastDice2 = session.LastDice2,
                PlayerScore = isPlayer1 ? session.Player1Score : session.Player2Score,
                OpponentScore = isPlayer1 ? session.Player2Score : session.Player1Score,
                CurrentRound = session.CurrentRound,
                IsAgainstBot = session.IsAgainstBot,
                OpponentName = isPlayer1
                    ? (session.IsAgainstBot ? "Bot" : session.Player2?.Username)
                    : session.Player1?.Username
            };
        }

        public bool ValidateMove(List<int> board, List<int> selection, int dice1, int dice2)
        {
            if (selection == null || selection.Count == 0) return false;

            // Check all selected numbers are on board
            foreach (var num in selection)
            {
                if (!board.Contains(num)) return false;
            }

            int sum = selection.Sum();
            int diceSum = dice1 + dice2;

            // Valid combinations
            return sum == dice1 || sum == dice2 || sum == diceSum;
        }

        public List<int> GetBotMove(List<int> board, int dice1, int dice2, string difficulty)
        {
            var possibleMoves = new List<List<int>>();
            int diceSum = dice1 + dice2;

            // Single die moves
            if (board.Contains(dice1))
                possibleMoves.Add(new List<int> { dice1 });
            if (board.Contains(dice2))
                possibleMoves.Add(new List<int> { dice2 });

            // Sum move
            if (diceSum <= 12 && board.Contains(diceSum))
                possibleMoves.Add(new List<int> { diceSum });

            // Two number combinations for dice values
            for (int i = 1; i < dice1; i++)
            {
                if (board.Contains(i) && board.Contains(dice1 - i))
                    possibleMoves.Add(new List<int> { i, dice1 - i });
            }

            for (int i = 1; i < dice2; i++)
            {
                if (board.Contains(i) && board.Contains(dice2 - i))
                    possibleMoves.Add(new List<int> { i, dice2 - i });
            }

            // Two number combinations for sum
            for (int i = 1; i < diceSum; i++)
            {
                if (board.Contains(i) && board.Contains(diceSum - i))
                    possibleMoves.Add(new List<int> { i, diceSum - i });
            }

            if (possibleMoves.Count == 0)
                return new List<int>();

            // Bot difficulty logic
            switch (difficulty)
            {
                case "Easy":
                    // Random move
                    return possibleMoves[SecureRandomInt(0, possibleMoves.Count)];

                case "Hard":
                    // Prefer moves that clear more numbers
                    return possibleMoves.OrderByDescending(m => m.Count).First();

                default: // Medium
                    // Prefer larger numbers
                    return possibleMoves.OrderByDescending(m => m.Sum()).First();
            }
        }

        public async Task EndGame(Guid sessionId, Guid? winnerId)
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null) return;

            session.State = GameSessionState.Completed;
            session.EndedAt = DateTime.UtcNow;
            session.WinnerId = winnerId;

            if (winnerId.HasValue)
            {
                var loserId = winnerId == session.Player1Id ? session.Player2Id : session.Player1Id;

                if (loserId.HasValue) // Not a bot
                {
                    await UpdateScores(winnerId.Value, loserId.Value,
                        session.Player1Score, session.Player2Score);
                }
                else if (session.Player1Id.HasValue) // Against bot
                {
                    await UpdateScoresAgainstBot(session.Player1Id.Value,
                        winnerId == session.Player1Id);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateScores(Guid winnerId, Guid loserId, int winnerScore, int loserScore)
        {
            var winner = await _context.Players.FindAsync(winnerId);
            var loser = await _context.Players.FindAsync(loserId);

            if (winner != null)
            {
                winner.TotalWins++;
                winner.CurrentStreak++;
                winner.BestStreak = Math.Max(winner.BestStreak, winner.CurrentStreak);

                // Calculate score based on performance
                int scoreGain = CalculateScore(true, winnerScore, loserScore);
                winner.TotalScore += scoreGain;
                winner.WeeklyScore += scoreGain;
                winner.MonthlyScore += scoreGain;

                // Experience
                winner.Experience += 100;
                CheckLevelUp(winner);
            }

            if (loser != null)
            {
                loser.TotalLosses++;
                loser.CurrentStreak = 0;

                int scoreGain = CalculateScore(false, loserScore, winnerScore);
                loser.TotalScore += scoreGain;
                loser.WeeklyScore += scoreGain;
                loser.MonthlyScore += scoreGain;

                loser.Experience += 25;
                CheckLevelUp(loser);
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateScoresAgainstBot(Guid playerId, bool won)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return;

            if (won)
            {
                player.TotalWins++;
                player.CurrentStreak++;
                player.BestStreak = Math.Max(player.BestStreak, player.CurrentStreak);

                int scoreGain = 50; // Base score for bot win
                player.TotalScore += scoreGain;
                player.WeeklyScore += scoreGain;
                player.MonthlyScore += scoreGain;

                player.Experience += 50;
            }
            else
            {
                player.TotalLosses++;
                player.CurrentStreak = 0;
                player.Experience += 10;
            }

            CheckLevelUp(player);
            await _context.SaveChangesAsync();
        }

        private int CalculateScore(bool won, int myScore, int opponentScore)
        {
            int baseScore = won ? 100 : 10;
            int scoreDiff = Math.Abs(myScore - opponentScore);
            int bonus = scoreDiff * 2;

            return baseScore + bonus;
        }

        private void CheckLevelUp(Player player)
        {
            int requiredExp = player.Level * 200;
            while (player.Experience >= requiredExp)
            {
                player.Experience -= requiredExp;
                player.Level++;
                requiredExp = player.Level * 200;
            }
        }

        private int SecureRandomInt(int min, int max)
        {
            byte[] bytes = new byte[4];
            _rng.GetBytes(bytes);
            int value = BitConverter.ToInt32(bytes, 0);
            return Math.Abs(value % (max - min)) + min;
        }

        private bool RandomBool()
        {
            return SecureRandomInt(0, 2) == 0;
        }
    }
}