using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShutTheTwelveServer.Data;
using ShutTheTwelveServer.Hubs;
using ShutTheTwelveServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ShutTheTwelveServer.Services
{
    public class MatchmakingRequest
    {
        public Guid PlayerId { get; set; }
        public string ConnectionId { get; set; }
        public string GameMode { get; set; }
        public List<int> SelectedCards { get; set; }
        public DateTime RequestTime { get; set; }
        public int Rating { get; set; }
    }

    public interface IMatchmakingService
    {
        Task<Guid?> FindMatch(MatchmakingRequest request);
        void CancelMatchmaking(Guid playerId);
        Task<GameSession> CreateBotMatch(Guid playerId, string gameMode);
    }

    public class MatchmakingService : IMatchmakingService, IHostedService
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<MatchmakingRequest>> _queues;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<GameHub> _hubContext;
        private Timer _matchmakingTimer;

        public MatchmakingService(IServiceProvider serviceProvider, IHubContext<GameHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _queues = new ConcurrentDictionary<string, ConcurrentQueue<MatchmakingRequest>>();
            _queues["Classic"] = new ConcurrentQueue<MatchmakingRequest>();
            _queues["Score"] = new ConcurrentQueue<MatchmakingRequest>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _matchmakingTimer = new Timer(ProcessMatchmaking, null,
                TimeSpan.Zero, TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _matchmakingTimer?.Dispose();
            return Task.CompletedTask;
        }

        public async Task<Guid?> FindMatch(MatchmakingRequest request)
        {
            if (!_queues.ContainsKey(request.GameMode))
                return null;

            var queue = _queues[request.GameMode];

            // Check for existing player in queue
            var existingRequests = queue.ToList();
            var match = existingRequests
                .Where(r => r.PlayerId != request.PlayerId)
                .Where(r => Math.Abs(r.Rating - request.Rating) <= 200) // Rating range
                .OrderBy(r => r.RequestTime)
                .FirstOrDefault();

            if (match != null)
            {
                // Remove matched player from queue
                var tempQueue = new ConcurrentQueue<MatchmakingRequest>();
                while (queue.TryDequeue(out var req))
                {
                    if (req.PlayerId != match.PlayerId)
                        tempQueue.Enqueue(req);
                }
                _queues[request.GameMode] = tempQueue;

                // Create game session
                using var scope = _serviceProvider.CreateScope();
                var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

                var session = await gameService.CreateGameSession(
                    request.PlayerId,
                    match.PlayerId,
                    request.GameMode,
                    false);

                // Notify both players
                await _hubContext.Clients.Client(request.ConnectionId)
                    .SendAsync("MatchFound", session.Id, match.PlayerId);
                await _hubContext.Clients.Client(match.ConnectionId)
                    .SendAsync("MatchFound", session.Id, request.PlayerId);

                return session.Id;
            }

            // Add to queue
            queue.Enqueue(request);
            return null;
        }

        public void CancelMatchmaking(Guid playerId)
        {
            foreach (var queue in _queues.Values)
            {
                var tempQueue = new ConcurrentQueue<MatchmakingRequest>();
                while (queue.TryDequeue(out var req))
                {
                    if (req.PlayerId != playerId)
                        tempQueue.Enqueue(req);
                }

                while (tempQueue.TryDequeue(out var req))
                {
                    queue.Enqueue(req);
                }
            }
        }

        public async Task<GameSession> CreateBotMatch(Guid playerId, string gameMode)
        {
            using var scope = _serviceProvider.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            return await gameService.CreateGameSession(playerId, null, gameMode, true);
        }

        private async void ProcessMatchmaking(object state)
        {
            var now = DateTime.UtcNow;

            foreach (var (gameMode, queue) in _queues)
            {
                var tempList = queue.ToList();
                foreach (var request in tempList)
                {
                    // If waiting more than 10 seconds, create bot match
                    if ((now - request.RequestTime).TotalSeconds > 10)
                    {
                        // Remove from queue
                        var tempQueue = new ConcurrentQueue<MatchmakingRequest>();
                        while (queue.TryDequeue(out var req))
                        {
                            if (req.PlayerId != request.PlayerId)
                                tempQueue.Enqueue(req);
                        }
                        _queues[gameMode] = tempQueue;

                        // Create bot match
                        var session = await CreateBotMatch(request.PlayerId, gameMode);

                        // Notify player
                        await _hubContext.Clients.Client(request.ConnectionId)
                            .SendAsync("BotMatchCreated", session.Id);
                    }
                }
            }
        }
    }
}