using ShutTheTwelve.Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace ShutTheBox.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; } = null!;

        public int SenderId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
