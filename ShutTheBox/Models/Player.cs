using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;
        public int TotalDraws { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public int BestStreak { get; set; } = 0;
        public int TotalScore { get; set; } = 0;
        public int WeeklyScore { get; set; } = 0;
        public int MonthlyScore { get; set; } = 0;
        public DateTime LastWeeklyReset { get; set; } = DateTime.UtcNow;
        public DateTime LastMonthlyReset { get; set; } = DateTime.UtcNow;

        public string ClientVersion { get; set; } = "1.0.0";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = false;
        public string ConnectionId { get; set; }

        // Navigation properties
        public virtual ICollection<GameSession> GameSessionsAsPlayer1 { get; set; }
        public virtual ICollection<GameSession> GameSessionsAsPlayer2 { get; set; }
        public virtual ICollection<PlayerCard> PlayerCards { get; set; }
        public virtual ICollection<MatchHistory> MatchHistories { get; set; }
    }
}