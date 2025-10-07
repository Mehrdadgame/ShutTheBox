using System;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public class PowerCardUsage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid GameSessionId { get; set; }
        public virtual GameSession GameSession { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int PowerCardId { get; set; }
        public virtual PowerCard PowerCard { get; set; }

        public int TurnNumber { get; set; }
        public string Effect { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}