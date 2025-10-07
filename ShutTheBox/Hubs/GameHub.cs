using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShutTheTwelveServer.Data;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Models;
using ShutTheTwelveServer.Services;

namespace ShutTheTwelveServer.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly GameDbContext _context;
        private readonly IGameService _gameService;
        private readonly IMatchmakingService _matchmakingService;
        private readonly ILeaderboardService _leaderboardService;
        private static readonly Dictionary<string, Guid> _connections = new();

        public GameHub(
            GameDbContext context,
            IGameService gameService,
            IMatchmakingService matchmakingService,
            ILeaderboardService leaderboardService)
        {
            _context = context;
            _gameService = gameService;
            _matchmakingService = matchmakingService;
            _leaderboardService = leaderboardService;
        }

        public override async Task OnConnectedAsync()
        {
            var playerId = GetPlayerId();
            if (playerId.HasValue)
            {
                _connections[Context.ConnectionId] = playerId.Value;

                var player = await _context.Players.FindAsync(playerId.Value);
                if (player != null)
                {
                    player.IsOnline = true;
                    player.ConnectionId = Context.ConnectionId;
                    await _context.SaveChangesAsync();
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, playerId.Value.ToString());
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var playerId))
            {
                _connections.Remove(Context.ConnectionId);

                var player = await _context.Players.FindAsync(playerId);
                if (player != null)
                {
                    player.IsOnline = false;
                    player.ConnectionId = null;
                    await _context.SaveChangesAsync();
                }

                _matchmakingService.CancelMatchmaking(playerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, playerId.ToString());
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task FindMatch(MatchmakingRequestDTO request)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            var player = await _context.Players.FindAsync(playerId.Value);
            if (player == null) return;

            var matchRequest = new MatchmakingRequest
            {
                PlayerId = playerId.Value,
                ConnectionId = Context.ConnectionId,
                GameMode = request.GameMode,
                SelectedCards = request.SelectedCards,
                RequestTime = DateTime.UtcNow,
                Rating = player.TotalScore
            };

            var sessionId = await _matchmakingService.FindMatch(matchRequest);

            if (sessionId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.Value.ToString());
            }
            else
            {
                await Clients.Caller.SendAsync("SearchingForMatch");
            }
        }

        public async Task CancelMatchmaking()
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            _matchmakingService.CancelMatchmaking(playerId.Value);
            await Clients.Caller.SendAsync("MatchmakingCancelled");
        }

        public async Task JoinGame(Guid sessionId)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            var session = await _context.GameSessions
                .Include(s => s.Player1)
                .Include(s => s.Player2)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return;

            // Verify player is part of this session
            if (session.Player1Id != playerId && session.Player2Id != playerId)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

            var gameState = await _gameService.GetGameState(sessionId, playerId.Value);
            await Clients.Caller.SendAsync("GameJoined", gameState);

            // Notify opponent
            var opponentId = session.Player1Id == playerId ? session.Player2Id : session.Player1Id;
            if (opponentId.HasValue)
            {
                await Clients.Group(opponentId.Value.ToString())
                    .SendAsync("OpponentJoined", playerId.Value);
            }
        }

        public async Task RollDice(Guid sessionId)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            try
            {
                var rollResult = await _gameService.RollDice(sessionId, playerId.Value);

                await Clients.Group(sessionId.ToString())
                    .SendAsync("DiceRolled", rollResult, playerId.Value);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task MakeMove(MoveDTO move)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            try
            {
                var success = await _gameService.MakeMove(move.SessionId, playerId.Value, move.SelectedNumbers);

                if (success)
                {
                    var gameState = await _gameService.GetGameState(move.SessionId, playerId.Value);

                    await Clients.Group(move.SessionId.ToString())
                        .SendAsync("MoveMade", gameState, playerId.Value);

                    // Check for game end
                    var session = await _context.GameSessions.FindAsync(move.SessionId);
                    if (session?.State == GameSessionState.Completed)
                    {
                        await Clients.Group(move.SessionId.ToString())
                            .SendAsync("GameEnded", session.WinnerId);
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("InvalidMove");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task UseCard(UseCardDTO cardData)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            try
            {
                // Record card usage
                var cardUsage = new PowerCardUsage
                {
                    Id = Guid.NewGuid(),
                    GameSessionId = cardData.SessionId,
                    PlayerId = playerId.Value,
                    PowerCardId = cardData.CardId,
                    UsedAt = DateTime.UtcNow
                };

                _context.PowerCardUsages.Add(cardUsage);
                await _context.SaveChangesAsync();

                // Notify all players in game
                await Clients.Group(cardData.SessionId.ToString())
                    .SendAsync("CardUsed", cardData.CardId, playerId.Value, cardData.Parameters);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SendMessage(Guid sessionId, string message)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            var player = await _context.Players.FindAsync(playerId.Value);
            if (player == null) return;

            await Clients.Group(sessionId.ToString())
                .SendAsync("MessageReceived", player.Username, message);
        }

        public async Task GetLeaderboard(string type)
        {
            var playerId = GetPlayerId();
            if (!playerId.HasValue) return;

            try
            {
                LeaderboardDTO leaderboard;

                if (type == "Weekly")
                    leaderboard = await _leaderboardService.GetWeeklyLeaderboard(playerId.Value);
                else
                    leaderboard = await _leaderboardService.GetMonthlyLeaderboard(playerId.Value);

                await Clients.Caller.SendAsync("LeaderboardReceived", leaderboard);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task CheckVersion(string clientVersion)
        {
            var currentVersion = "1.0.0"; // Get from config

            if (clientVersion != currentVersion)
            {
                await Clients.Caller.SendAsync("ForceUpdate", currentVersion);
            }
            else
            {
                await Clients.Caller.SendAsync("VersionOk");
            }
        }

        private Guid? GetPlayerId()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}