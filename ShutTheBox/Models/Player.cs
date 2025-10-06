using ShutTheTwelve.Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveBackend.Models
{
    // Player Entity
    public class Player
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public int Power { get; set; } = 100;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<GameSession> GamesAsPlayer1 { get; set; } = new List<GameSession>();
        public ICollection<GameSession> GamesAsPlayer2 { get; set; } = new List<GameSession>();
    }
}