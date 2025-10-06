using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelve.Backend.Models
{
    public class DiceRoll
    {
        [Key]
        public int Id { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; } = null!;

        public int Roll1 { get; set; }
        public int Roll2 { get; set; }
        public string PlayerTurn { get; set; } = string.Empty; // Player1 or Player2

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}