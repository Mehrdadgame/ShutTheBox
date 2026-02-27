using System;
using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.Models
{
    public enum CardType
    {
        LockAndReroll,
        Sabotage,
        SecondChance,
        WildDice,
        StealTurn,
        Shield,
        LightningRoll,
        SwapBoard,
        Mimic
    }

    public enum CardRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public class PowerCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public CardType Type { get; set; }
        public CardRarity Rarity { get; set; }

        public int PowerCost { get; set; }
        public int UnlockLevel { get; set; } = 1;

        [Required]
        public string IconUrl { get; set; } = string.Empty; // Fix: Initialize with a default value

        public bool IsActive { get; set; } = true;

        public virtual ICollection<PlayerCard> PlayerCards { get; set; }
    }
}