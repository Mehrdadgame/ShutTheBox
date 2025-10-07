using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Services;

namespace ShutTheTwelveServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILeaderboardService _leaderboardService;

        public GameController(IGameService gameService, ILeaderboardService leaderboardService)
        {
            _gameService = gameService;
            _leaderboardService = leaderboardService;
        }

        [HttpGet("state/{sessionId}")]
        public async Task<IActionResult> GetGameState(Guid sessionId)
        {
            var playerId = GetUserId();
            if (!playerId.HasValue) return Unauthorized();

            var state = await _gameService.GetGameState(sessionId, playerId.Value);
            if (state == null) return NotFound();

            return Ok(state);
        }

        [HttpGet("leaderboard/weekly")]
        public async Task<IActionResult> GetWeeklyLeaderboard()
        {
            var playerId = GetUserId();
            if (!playerId.HasValue) return Unauthorized();

            var leaderboard = await _leaderboardService.GetWeeklyLeaderboard(playerId.Value);
            return Ok(leaderboard);
        }

        [HttpGet("leaderboard/monthly")]
        public async Task<IActionResult> GetMonthlyLeaderboard()
        {
            var playerId = GetUserId();
            if (!playerId.HasValue) return Unauthorized();

            var leaderboard = await _leaderboardService.GetMonthlyLeaderboard(playerId.Value);
            return Ok(leaderboard);
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}