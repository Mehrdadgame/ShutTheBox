using Microsoft.EntityFrameworkCore;
using ShutTheBox.Models;
using ShutTheTwelve.Backend.Models;
using ShutTheTwelveBackend.Models;

namespace ShutTheTwelveBackend.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<DiceRoll> DiceRolls { get; set; }
        public DbSet<PowerCard> PowerCards { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Player Configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Username).IsUnique();
                entity.Property(p => p.Username).HasMaxLength(50);
            });

            // GameSession Configuration
            modelBuilder.Entity<GameSession>(entity =>
            {
                entity.HasKey(g => g.Id);

                entity.HasOne(g => g.Player1)
                    .WithMany(p => p.GamesAsPlayer1)
                    .HasForeignKey(g => g.Player1Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Player2)
                    .WithMany(p => p.GamesAsPlayer2)
                    .HasForeignKey(g => g.Player2Id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DiceRoll Configuration
            modelBuilder.Entity<DiceRoll>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.HasOne(d => d.GameSession)
                    .WithMany(g => g.DiceRolls)
                    .HasForeignKey(d => d.GameSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Message Configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasOne(m => m.GameSession)
                    .WithMany(g => g.Messages)
                    .HasForeignKey(m => m.GameSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PowerCard Configuration
            modelBuilder.Entity<PowerCard>(entity =>
            {
                entity.HasKey(pc => pc.Id);
            });

            // Seed Initial Power Cards
            SeedPowerCards(modelBuilder);
        }

        private void SeedPowerCards(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PowerCard>().HasData(
                new PowerCard
                {
                    Id = 1,
                    Name = "Lock & Reroll",
                    Description = "Lock one die and reroll the other",
                    Cost = 20,
                    EffectType = "LockAndReroll",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 2,
                    Name = "Sabotage",
                    Description = "Opponent's next die rolls 1-3",
                    Cost = 25,
                    EffectType = "Sabotage",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 3,
                    Name = "Second Chance",
                    Description = "Reroll if no moves available",
                    Cost = 15,
                    EffectType = "SecondChance",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 4,
                    Name = "Wild Die",
                    Description = "Set one die to 5 or 6",
                    Cost = 30,
                    EffectType = "WildDice",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 5,
                    Name = "Steal Turn",
                    Description = "Steal opponent's roll result",
                    Cost = 35,
                    EffectType = "StealTurn",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 6,
                    Name = "Shield",
                    Description = "Block opponent's attack card",
                    Cost = 15,
                    EffectType = "Shield",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 7,
                    Name = "Lightning Roll",
                    Description = "Roll 3 times, pick best",
                    Cost = 40,
                    EffectType = "LightningRoll",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 8,
                    Name = "Swap Board",
                    Description = "Swap one number with opponent",
                    Cost = 30,
                    EffectType = "SwapBoard",
                    IsActive = true
                },
                new PowerCard
                {
                    Id = 9,
                    Name = "Mimic",
                    Description = "Copy opponent's last card",
                    Cost = 25,
                    EffectType = "Mimic",
                    IsActive = true
                }
            );
        }
    }
}