using System;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public class MatchHistory
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public Guid GameSessionId { get; set; }
        public virtual GameSession GameSession { get; set; }

        public Guid? OpponentId { get; set; }
        public string OpponentName { get; set; }

        public bool IsWinner { get; set; }
        public int ScoreGained { get; set; }
        public int ExpGained { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}