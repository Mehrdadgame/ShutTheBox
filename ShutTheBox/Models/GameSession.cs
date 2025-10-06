using ShutTheBox.Models;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelveBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelve.Backend.Models
{
    public class GameSession
    {
        [Key]
        public int Id { get; set; }

        public int Player1Id { get; set; }
        public Player Player1 { get; set; } = null!;

        public int? Player2Id { get; set; }
        public Player? Player2 { get; set; }

        public bool IsAIGame { get; set; } = false;

        [Required]
        public string GameMode { get; set; } = "Classic"; // Classic or Score

        [Required]
        public string State { get; set; } = "Waiting"; // Waiting, InProgress, Finished

        public string ActiveTurn { get; set; } = "Player1"; // Player1 or Player2

        // Game State Data (JSON serialized)
        public string Player1Board { get; set; } = "[1,2,3,4,5,6,7,8,9,10,11,12]";
        public string Player2Board { get; set; } = "[1,2,3,4,5,6,7,8,9,10,11,12]";
        public string Player1Cards { get; set; } = "[]";
        public string Player2Cards { get; set; } = "[]";

        public int Player1Score { get; set; } = 0;
        public int Player2Score { get; set; } = 0;
        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 10;

        public int? WinnerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }

        // Navigation Properties
        public ICollection<DiceRoll> DiceRolls { get; set; } = new List<DiceRoll>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}