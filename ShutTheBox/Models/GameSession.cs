using ShutTheTwelve.Backend.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public enum GameSessionState
    {
        Waiting,
        InProgress,
        Completed,
        Abandoned
    }

    public enum PlayerTurn
    {
        Player1,
        Player2
    }

    public class GameSession
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? Player1Id { get; set; }
        public virtual Player Player1 { get; set; }

        public Guid? Player2Id { get; set; }
        public virtual Player Player2 { get; set; }

        public bool IsAgainstBot { get; set; } = false;
        public string BotDifficulty { get; set; } = "Medium";

        public GameSessionState State { get; set; } = GameSessionState.Waiting;
        public PlayerTurn CurrentTurn { get; set; } = PlayerTurn.Player1;

        public string Player1Board { get; set; } = "1,2,3,4,5,6,7,8,9,10,11,12";
        public string Player2Board { get; set; } = "1,2,3,4,5,6,7,8,9,10,11,12";

        public int CurrentRound { get; set; } = 1;
        public int Player1Score { get; set; } = 0;
        public int Player2Score { get; set; } = 0;

        public int LastDice1 { get; set; }
        public int LastDice2 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public Guid? WinnerId { get; set; }
        public string GameMode { get; set; } = "Classic";

        // Navigation properties
        public virtual ICollection<GameMove> GameMoves { get; set; }
        public virtual ICollection<PowerCardUsage> PowerCardUsages { get; set; }
    }
}