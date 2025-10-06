using ShutTheBox.DTOs;
using ShutTheTwelve.Backend.Interfaces;
using System.Security.Cryptography;

namespace ShutTheTwelve.Backend.Services
{
    public class BotService : IBotService
    {
        public async Task<List<int>> GetBotMove(List<int> board, int die1, int die2)
        {
            await Task.Delay(500); // Simulate thinking time

            List<int> possibleMoves = new();
            int sum = die1 + die2;

            // Strategy 1: Try to use the sum
            if (sum <= 12 && board.Contains(sum))
            {
                possibleMoves.Add(sum);
                return possibleMoves;
            }

            // Strategy 2: Try to use both dice
            if (board.Contains(die1) && board.Contains(die2))
            {
                possibleMoves.Add(die1);
                possibleMoves.Add(die2);
                return possibleMoves;
            }

            // Strategy 3: Use larger die
            int larger = Math.Max(die1, die2);
            int smaller = Math.Min(die1, die2);

            if (board.Contains(larger))
            {
                possibleMoves.Add(larger);
                return possibleMoves;
            }

            if (board.Contains(smaller))
            {
                possibleMoves.Add(smaller);
                return possibleMoves;
            }

            // Strategy 4: Find any valid combination
            for (int i = 0; i < board.Count; i++)
            {
                if (board[i] <= sum)
                {
                    int remaining = sum - board[i];
                    if (remaining > 0 && board.Contains(remaining) && remaining != board[i])
                    {
                        possibleMoves.Add(board[i]);
                        possibleMoves.Add(remaining);
                        return possibleMoves;
                    }
                }
            }

            return possibleMoves;
        }

        public bool ShouldUseCard(string cardType, GameStateDto gameState)
        {
            // Simple AI decision making (30% chance to use card)
            int randomChance = RandomNumberGenerator.GetInt32(0, 100);

            switch (cardType)
            {
                case "Sabotage":
                case "WildDice":
                    return randomChance < 30;

                case "SecondChance":
                    // Only use if no moves available
                    return randomChance < 50;

                case "LightningRoll":
                    return randomChance < 25;

                default:
                    return randomChance < 20;
            }
        }
    }
}