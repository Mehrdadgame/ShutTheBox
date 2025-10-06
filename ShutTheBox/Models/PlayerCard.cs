using ShutTheTwelveBackend.Models;

namespace ShutTheTwelve.Backend.Models
{
    public class PlayerCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public string CardId { get; set; }
        public virtual PowerCard Card { get; set; }

        public bool Used { get; set; } = false;
        public DateTime? UsedAt { get; set; }
    }
}