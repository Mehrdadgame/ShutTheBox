using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Services;

namespace ShutTheTwelveServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var response = await _authService.Register(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var response = await _authService.Login(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        [HttpGet("version")]
        public IActionResult CheckVersion([FromQuery] string clientVersion)
        {
            var isValid = _authService.CheckVersion(clientVersion);
            return Ok(new
            {
                requiresUpdate = !isValid,
                currentVersion = "1.0.0"
            });
        }
    }
}