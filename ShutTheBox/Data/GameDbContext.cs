using Microsoft.EntityFrameworkCore;
using ShutTheTwelveServer.Models;

namespace ShutTheTwelveServer.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<PowerCard> PowerCards { get; set; }
        public DbSet<PlayerCard> PlayerCards { get; set; }
        public DbSet<GameMove> GameMoves { get; set; }
        public DbSet<MatchHistory> MatchHistories { get; set; }
        public DbSet<PowerCardUsage> PowerCardUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Player relationships
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Username)
                .IsUnique();

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Email)
                .IsUnique();

            // GameSession relationships
            modelBuilder.Entity<GameSession>()
                .HasOne(g => g.Player1)
                .WithMany(p => p.GameSessionsAsPlayer1)
                .HasForeignKey(g => g.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSession>()
                .HasOne(g => g.Player2)
                .WithMany(p => p.GameSessionsAsPlayer2)
                .HasForeignKey(g => g.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // PlayerCard composite key
            modelBuilder.Entity<PlayerCard>()
                .HasKey(pc => new { pc.PlayerId, pc.PowerCardId });

            // Seed power cards
            modelBuilder.Entity<PowerCard>().HasData(
                new PowerCard { Id = 1, Name = "Lock & Reroll", Type = CardType.LockAndReroll, Rarity = CardRarity.Common, PowerCost = 15, Description = "Lock one die and reroll the other" },
                new PowerCard { Id = 2, Name = "Sabotage", Type = CardType.Sabotage, Rarity = CardRarity.Common, PowerCost = 20, Description = "Force opponent's die to roll 1-3" },
                new PowerCard { Id = 3, Name = "Second Chance", Type = CardType.SecondChance, Rarity = CardRarity.Rare, PowerCost = 25, Description = "Reroll both dice if no moves" },
                new PowerCard { Id = 4, Name = "Wild Dice", Type = CardType.WildDice, Rarity = CardRarity.Rare, PowerCost = 30, Description = "Set one die to 5 or 6" },
                new PowerCard { Id = 5, Name = "Steal Turn", Type = CardType.StealTurn, Rarity = CardRarity.Epic, PowerCost = 35, Description = "Steal opponent's dice roll" },
                new PowerCard { Id = 6, Name = "Shield", Type = CardType.Shield, Rarity = CardRarity.Epic, PowerCost = 25, Description = "Block opponent's attack card" },
                new PowerCard { Id = 7, Name = "Lightning Roll", Type = CardType.LightningRoll, Rarity = CardRarity.Legendary, PowerCost = 40, Description = "Roll 3 times, keep best" },
                new PowerCard { Id = 8, Name = "Swap Board", Type = CardType.SwapBoard, Rarity = CardRarity.Epic, PowerCost = 30, Description = "Swap random numbers with opponent" },
                new PowerCard { Id = 9, Name = "Mimic", Type = CardType.Mimic, Rarity = CardRarity.Legendary, PowerCost = 35, Description = "Copy opponent's last card" }
            );
        }
    }
}