using ShutTheTwelve.Backend.Enums;
using ShutTheTwelveBackend.Models;

namespace ShutTheTwelve.Backend.Models
{
    public class CardUsage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string GameSessionId { get; set; }
        public virtual GameSession GameSession { get; set; }

        public string PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public CardType CardType { get; set; }
        public string AdditionalDataJson { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}