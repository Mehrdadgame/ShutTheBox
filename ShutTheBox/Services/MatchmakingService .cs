using ShutTheTwelve.Backend.Interfaces;
using System.Collections.Concurrent;

namespace ShutTheBox.Services
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ConcurrentDictionary<int, MatchmakingRequest> _queue = new();
        private readonly TimeSpan _maxWaitTime = TimeSpan.FromSeconds(10);

        public async Task<MatchmakingResult> FindMatch(int playerId, string gameMode)
        {
            // Add player to queue
            var request = new MatchmakingRequest
            {
                PlayerId = playerId,
                GameMode = gameMode,
                QueueTime = DateTime.UtcNow
            };

            _queue.TryAdd(playerId, request);

            // Try to find a match
            var startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - startTime) < _maxWaitTime)
            {
                // Look for another player in queue with same game mode
                var opponent = _queue.Values
                    .FirstOrDefault(r => r.PlayerId != playerId &&
                                       r.GameMode == gameMode &&
                                       r.PlayerId != playerId);

                if (opponent != null)
                {
                    // Match found! Remove both players from queue
                    _queue.TryRemove(playerId, out _);
                    _queue.TryRemove(opponent.PlayerId, out _);

                    return new MatchmakingResult
                    {
                        MatchFound = true,
                        IsAIMatch = false,
                        OpponentId = opponent.PlayerId,
                        Message = "Match found with another player!"
                    };
                }

                await Task.Delay(500); // Check every 500ms
            }

            // Timeout - create AI match
            _queue.TryRemove(playerId, out _);

            return new MatchmakingResult
            {
                MatchFound = true,
                IsAIMatch = true,
                OpponentId = null,
                Message = "No players found. Starting match with AI."
            };
        }

        public void CancelMatchmaking(int playerId)
        {
            _queue.TryRemove(playerId, out _);
        }

        public bool IsPlayerInQueue(int playerId)
        {
            return _queue.ContainsKey(playerId);
        }
    }
}

