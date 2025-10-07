using System;

namespace ShutTheTwelveServer.Models
{
    public class PlayerCard
    {
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int PowerCardId { get; set; }
        public virtual PowerCard PowerCard { get; set; }

        public int Quantity { get; set; } = 1;
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}