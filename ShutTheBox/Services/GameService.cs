using Microsoft.EntityFrameworkCore;
using ShutTheBox.DTOs;
using ShutTheBox.Models;
using ShutTheTwelve.Backend.Interfaces;
using ShutTheTwelve.Backend.Models;
using ShutTheTwelveBackend.Data;
using System.Security.Cryptography;
using System.Text.Json;

namespace ShutTheTwelve.Backend.Services
{
    public class GameService : IGameService
    {
        private readonly GameDbContext _context;

        public GameService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<GameSession> CreateGame(int player1Id, int? player2Id, string gameMode, bool isAI)
        {
            var game = new GameSession
            {
                Player1Id = player1Id,
                Player2Id = player2Id,
                IsAIGame = isAI,
                GameMode = gameMode,
                State = "InProgress",
                ActiveTurn = "Player1",
                Player1Board = "[1,2,3,4,5,6,7,8,9,10,11,12]",
                Player2Board = "[1,2,3,4,5,6,7,8,9,10,11,12]",
                Player1Cards = "[]",
                Player2Cards = "[]",
                CreatedAt = DateTime.UtcNow,
                CurrentRound = 1,
                TotalRounds = gameMode == "Score" ? 10 : 1
            };

            _context.GameSessions.Add(game);
            await _context.SaveChangesAsync();

            return game;
        }

        public async Task<GameSession?> GetGameById(int gameId)
        {
            return await _context.GameSessions
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task<DiceRollResult> RollDice(int gameId)
        {
            var game = await GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            // Secure server-side dice rolling
            int die1 = RandomNumberGenerator.GetInt32(1, 7);
            int die2 = RandomNumberGenerator.GetInt32(1, 7);

            var diceRoll = new DiceRoll
            {
                GameSessionId = gameId,
                Roll1 = die1,
                Roll2 = die2,
                PlayerTurn = game.ActiveTurn,
                Timestamp = DateTime.UtcNow
            };

            _context.DiceRolls.Add(diceRoll);
            await _context.SaveChangesAsync();

            return new DiceRollResult
            {
                Die1 = die1,
                Die2 = die2,
                PlayerTurn = game.ActiveTurn
            };
        }

        public async Task<bool> MakeMove(int gameId, int playerId, List<int> selectedNumbers)
        {
            var game = await GetGameById(gameId);
            if (game == null) return false;

            // Get last dice roll
            var lastRoll = await _context.DiceRolls
                .Where(d => d.GameSessionId == gameId)
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync();

            if (lastRoll == null) return false;

            // Determine which board to update
            List<int> board;
            string boardJson;

            if (game.Player1Id == playerId && game.ActiveTurn == "Player1")
            {
                board = JsonSerializer.Deserialize<List<int>>(game.Player1Board) ?? new List<int>();
            }
            else if (game.Player2Id == playerId && game.ActiveTurn == "Player2")
            {
                board = JsonSerializer.Deserialize<List<int>>(game.Player2Board) ?? new List<int>();
            }
            else
            {
                return false; // Not player's turn
            }

            // Validate move
            if (!ValidateMove(board, selectedNumbers, lastRoll.Roll1, lastRoll.Roll2))
                return false;

            // Remove selected numbers from board
            foreach (var num in selectedNumbers)
            {
                board.Remove(num);
            }

            boardJson = JsonSerializer.Serialize(board);

            // Update game state
            if (game.Player1Id == playerId)
            {
                game.Player1Board = boardJson;
            }
            else
            {
                game.Player2Board = boardJson;
            }

            // Check for winner
            if (board.Count == 0)
            {
                game.State = "Finished";
                game.WinnerId = playerId;
                game.FinishedAt = DateTime.UtcNow;

                // Update player stats
                var winner = await _context.Players.FindAsync(playerId);
                if (winner != null)
                {
                    winner.Wins++;
                }

                if (!game.IsAIGame && game.Player2Id.HasValue)
                {
                    var loser = await _context.Players.FindAsync(
                        playerId == game.Player1Id ? game.Player2Id.Value : game.Player1Id);
                    if (loser != null)
                    {
                        loser.Losses++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EndTurn(int gameId)
        {
            var game = await GetGameById(gameId);
            if (game == null) return false;

            // Switch turn
            game.ActiveTurn = game.ActiveTurn == "Player1" ? "Player2" : "Player1";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GameStateDto> GetGameState(int gameId)
        {
            var game = await GetGameById(gameId);
            if (game == null)
                throw new Exception("Game not found");

            return new GameStateDto
            {
                GameId = game.Id,
                GameMode = game.GameMode,
                State = game.State,
                ActiveTurn = game.ActiveTurn,
                IsAIGame = game.IsAIGame,
                Player1Board = JsonSerializer.Deserialize<List<int>>(game.Player1Board) ?? new(),
                Player2Board = JsonSerializer.Deserialize<List<int>>(game.Player2Board) ?? new(),
                Player1Cards = JsonSerializer.Deserialize<List<string>>(game.Player1Cards) ?? new(),
                Player2Cards = JsonSerializer.Deserialize<List<string>>(game.Player2Cards) ?? new(),
                Player1Score = game.Player1Score,
                Player2Score = game.Player2Score,
                CurrentRound = game.CurrentRound,
                TotalRounds = game.TotalRounds,
                WinnerId = game.WinnerId
            };
        }

        public async Task<bool> UseCard(int gameId, int playerId, string cardType, int? targetDie = null)
        {
            var game = await GetGameById(gameId);
            if (game == null) return false;

            // بررسی نوبت
            bool isPlayer1 = game.Player1Id == playerId;
            string expectedTurn = isPlayer1 ? "Player1" : "Player2";

            // دریافت آخرین تاس
            var lastRoll = await _context.DiceRolls
                .Where(d => d.GameSessionId == gameId)
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefaultAsync();

            // پیاده‌سازی کارت‌ها
            switch (cardType.ToLower())
            {
                case "lockreroll":
                case "lockandreroll":
                    // قفل یک تاس و reroll تاس دیگر
                    if (lastRoll == null || targetDie == null) return false;

                    int newDie = RandomNumberGenerator.GetInt32(1, 7);
                    if (targetDie == 1)
                        lastRoll.Roll2 = newDie;
                    else
                        lastRoll.Roll1 = newDie;

                    await _context.SaveChangesAsync();
                    break;

                case "sabotage":
                    // خرابکاری - تاس حریف 1-3 می‌شود
                    // این باید در RollDice بعدی اعمال شود
                    // ذخیره در یک فیلد موقت
                    if (isPlayer1)
                        game.Player2Cards = game.Player2Cards + "|SABOTAGED:" + (targetDie ?? 1);
                    else
                        game.Player1Cards = game.Player1Cards + "|SABOTAGED:" + (targetDie ?? 1);

                    await _context.SaveChangesAsync();
                    break;

                case "secondchance":
                    // شانس دوم - reroll کامل
                    if (lastRoll != null)
                    {
                        lastRoll.Roll1 = RandomNumberGenerator.GetInt32(1, 7);
                        lastRoll.Roll2 = RandomNumberGenerator.GetInt32(1, 7);
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "wilddie":
                case "wilddice":
                    // تنظیم یک تاس به عدد دلخواه
                    if (lastRoll == null || targetDie == null) return false;

                    int wildValue = RandomNumberGenerator.GetInt32(5, 7); // 5 یا 6
                    if (targetDie == 1)
                        lastRoll.Roll1 = wildValue;
                    else
                        lastRoll.Roll2 = wildValue;

                    await _context.SaveChangesAsync();
                    break;

                case "stealturn":
                    // دزدیدن نوبت - تغییر نوبت به سود بازیکن
                    game.ActiveTurn = expectedTurn;
                    await _context.SaveChangesAsync();
                    break;

                case "shield":
                    // سپر - خنثی کردن کارت تهاجمی
                    // این باید reactive باشد
                    break;

                case "lightningroll":
                    // 3 بار تاس انداختن و انتخاب بهترین
                    var roll1 = (RandomNumberGenerator.GetInt32(1, 7), RandomNumberGenerator.GetInt32(1, 7));
                    var roll2 = (RandomNumberGenerator.GetInt32(1, 7), RandomNumberGenerator.GetInt32(1, 7));
                    var roll3 = (RandomNumberGenerator.GetInt32(1, 7), RandomNumberGenerator.GetInt32(1, 7));

                    var bestRoll = new[] { roll1, roll2, roll3 }
                        .OrderByDescending(r => r.Item1 + r.Item2)
                        .First();

                    if (lastRoll != null)
                    {
                        lastRoll.Roll1 = bestRoll.Item1;
                        lastRoll.Roll2 = bestRoll.Item2;
                        await _context.SaveChangesAsync();
                    }
                    break;

                case "swapboard":
                    // تعویض یک عدد با حریف
                    var playerBoard = isPlayer1 ?
                        JsonSerializer.Deserialize<List<int>>(game.Player1Board) :
                        JsonSerializer.Deserialize<List<int>>(game.Player2Board);
                    var opponentBoard = isPlayer1 ?
                        JsonSerializer.Deserialize<List<int>>(game.Player2Board) :
                        JsonSerializer.Deserialize<List<int>>(game.Player1Board);

                    if (playerBoard.Count > 0 && opponentBoard.Count > 0)
                    {
                        int playerNum = playerBoard[RandomNumberGenerator.GetInt32(0, playerBoard.Count)];
                        int opponentNum = opponentBoard[RandomNumberGenerator.GetInt32(0, opponentBoard.Count)];

                        playerBoard.Remove(playerNum);
                        opponentBoard.Remove(opponentNum);
                        playerBoard.Add(opponentNum);
                        opponentBoard.Add(playerNum);

                        if (isPlayer1)
                        {
                            game.Player1Board = JsonSerializer.Serialize(playerBoard);
                            game.Player2Board = JsonSerializer.Serialize(opponentBoard);
                        }
                        else
                        {
                            game.Player2Board = JsonSerializer.Serialize(playerBoard);
                            game.Player1Board = JsonSerializer.Serialize(opponentBoard);
                        }

                        await _context.SaveChangesAsync();
                    }
                    break;

                case "mimic":
                    // تقلید - کپی کردن آخرین کارت حریف
                    // نیاز به ذخیره تاریخچه کارت‌ها
                    break;
            }

            return true;
        }

        public bool ValidateMove(List<int> board, List<int> selection, int die1, int die2)
        {
            if (selection == null || selection.Count == 0) return false;

            // Check all selected numbers exist on board
            foreach (var num in selection)
            {
                if (!board.Contains(num)) return false;
            }

            int sum = selection.Sum();
            int diceSum = die1 + die2;

            // Valid if sum equals die1, die2, or die1+die2
            return sum == die1 || sum == die2 || sum == diceSum;
        }

        public bool HasPossibleMoves(List<int> board, int die1, int die2)
        {
            if (board.Count == 0) return false;

            int sum = die1 + die2;

            // Check if any subset sums to die1, die2, or sum
            return ExistsSubsetSum(board, die1) ||
                   ExistsSubsetSum(board, die2) ||
                   (sum <= 12 && ExistsSubsetSum(board, sum));
        }

        private bool ExistsSubsetSum(List<int> board, int target)
        {
            if (target <= 0 || target > 12) return false;

            int n = Math.Min(board.Count, 12);
            int maxMask = 1 << n;

            for (int mask = 1; mask < maxMask; mask++)
            {
                int sum = 0;
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0)
                        sum += board[i];
                }
                if (sum == target) return true;
            }

            return false;
        }
    }
}