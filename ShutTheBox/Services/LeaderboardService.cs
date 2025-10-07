using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShutTheTwelveServer.Data;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Models;

namespace ShutTheTwelveServer.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardDTO> GetWeeklyLeaderboard(Guid playerId);
        Task<LeaderboardDTO> GetMonthlyLeaderboard(Guid playerId);
        Task ResetWeeklyScores();
        Task ResetMonthlyScores();
    }

    public class LeaderboardService : ILeaderboardService, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _resetTimer;

        public LeaderboardService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _resetTimer = new Timer(CheckAndResetScores, null,
                TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _resetTimer?.Dispose();
            return Task.CompletedTask;
        }

        public async Task<LeaderboardDTO> GetWeeklyLeaderboard(Guid playerId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var topPlayers = await context.Players
                .OrderByDescending(p => p.WeeklyScore)
                .Take(100)
                .Select(p => new LeaderboardEntryDTO
                {
                    Username = p.Username,
                    Score = p.WeeklyScore,
                    Level = p.Level,
                    Wins = p.TotalWins,
                    Losses = p.TotalLosses,
                    WinRate = p.TotalWins + p.TotalLosses > 0
                        ? (float)p.TotalWins / (p.TotalWins + p.TotalLosses) * 100
                        : 0
                })
                .ToListAsync();

            // Add ranks
            for (int i = 0; i < topPlayers.Count; i++)
            {
                topPlayers[i].Rank = i + 1;
            }

            // Get player's entry
            var player = await context.Players.FindAsync(playerId);
            LeaderboardEntryDTO playerEntry = null;

            if (player != null)
            {
                var playerRank = await context.Players
                    .CountAsync(p => p.WeeklyScore > player.WeeklyScore) + 1;

                playerEntry = new LeaderboardEntryDTO
                {
                    Rank = playerRank,
                    Username = player.Username,
                    Score = player.WeeklyScore,
                    Level = player.Level,
                    Wins = player.TotalWins,
                    Losses = player.TotalLosses,
                    WinRate = player.TotalWins + player.TotalLosses > 0
                        ? (float)player.TotalWins / (player.TotalWins + player.TotalLosses) * 100
                        : 0
                };
            }

            var nextReset = GetNextWeeklyReset();

            return new LeaderboardDTO
            {
                Type = "Weekly",
                Entries = topPlayers,
                PlayerEntry = playerEntry,
                LastReset = nextReset.AddDays(-7),
                NextReset = nextReset
            };
        }

        public async Task<LeaderboardDTO> GetMonthlyLeaderboard(Guid playerId)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var topPlayers = await context.Players
                .OrderByDescending(p => p.MonthlyScore)
                .Take(100)
                .Select(p => new LeaderboardEntryDTO
                {
                    Username = p.Username,
                    Score = p.MonthlyScore,
                    Level = p.Level,
                    Wins = p.TotalWins,
                    Losses = p.TotalLosses,
                    WinRate = p.TotalWins + p.TotalLosses > 0
                        ? (float)p.TotalWins / (p.TotalWins + p.TotalLosses) * 100
                        : 0
                })
                .ToListAsync();

            // Add ranks
            for (int i = 0; i < topPlayers.Count; i++)
            {
                topPlayers[i].Rank = i + 1;
            }

            // Get player's entry
            var player = await context.Players.FindAsync(playerId);
            LeaderboardEntryDTO playerEntry = null;

            if (player != null)
            {
                var playerRank = await context.Players
                    .CountAsync(p => p.MonthlyScore > player.MonthlyScore) + 1;

                playerEntry = new LeaderboardEntryDTO
                {
                    Rank = playerRank,
                    Username = player.Username,
                    Score = player.MonthlyScore,
                    Level = player.Level,
                    Wins = player.TotalWins,
                    Losses = player.TotalLosses,
                    WinRate = player.TotalWins + player.TotalLosses > 0
                        ? (float)player.TotalWins / (player.TotalWins + player.TotalLosses) * 100
                        : 0
                };
            }

            var nextReset = GetNextMonthlyReset();

            return new LeaderboardDTO
            {
                Type = "Monthly",
                Entries = topPlayers,
                PlayerEntry = playerEntry,
                LastReset = nextReset.AddMonths(-1),
                NextReset = nextReset
            };
        }

        public async Task ResetWeeklyScores()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var players = await context.Players.ToListAsync();
            foreach (var player in players)
            {
                player.WeeklyScore = 0;
                player.LastWeeklyReset = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        public async Task ResetMonthlyScores()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            var players = await context.Players.ToListAsync();
            foreach (var player in players)
            {
                player.MonthlyScore = 0;
                player.LastMonthlyReset = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private async void CheckAndResetScores(object state)
        {
            var now = DateTime.UtcNow;

            // Check weekly reset (Every Monday at 00:00 UTC)
            if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 0)
            {
                await ResetWeeklyScores();
            }

            // Check monthly reset (First day of month at 00:00 UTC)
            if (now.Day == 1 && now.Hour == 0)
            {
                await ResetMonthlyScores();
            }
        }

        private DateTime GetNextWeeklyReset()
        {
            var now = DateTime.UtcNow;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0) daysUntilMonday = 7;
            return now.Date.AddDays(daysUntilMonday);
        }

        private DateTime GetNextMonthlyReset()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, 1).AddMonths(1);
        }
    }
}