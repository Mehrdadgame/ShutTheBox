using System;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public class GameMove
    {
        [Key]
        public Guid Id { get; set; }

        public Guid GameSessionId { get; set; }
        public virtual GameSession GameSession { get; set; }

        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        public int MoveNumber { get; set; }
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }

        public string NumbersUsed { get; set; } // "1,2,3" format
        public string BoardStateBefore { get; set; }
        public string BoardStateAfter { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}