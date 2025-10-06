using ShutTheTwelveBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelve.Backend.Models
{
    public class GameMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string GameSessionId { get; set; }
        public virtual GameSession GameSession { get; set; }

        public string PlayerId { get; set; }
        public virtual Player Player { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}