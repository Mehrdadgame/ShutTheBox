using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShutTheBox.DTOs;
using ShutTheBox.Models;
using ShutTheTwelve.Backend.Data;
using ShutTheTwelve.Backend.Interfaces;
using ShutTheTwelve.Backend.Models;
using ShutTheTwelveBackend.Data;
using ShutTheTwelveBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShutTheTwelve.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly GameDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(GameDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            // Check if username exists
            var existingUser = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == request.Username);

            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            // Create new player
            var player = new Player
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                Power = 100,
                Wins = 0,
                Losses = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(player);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                Player = new PlayerDto
                {
                    Id = player.Id,
                    Username = player.Username,
                    Power = player.Power,
                    Wins = player.Wins,
                    Losses = player.Losses
                }
            };
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == request.Username);

            if (player == null || !VerifyPassword(request.Password, player.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var token = GenerateJwtToken(player);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Player = new PlayerDto
                {
                    Id = player.Id,
                    Username = player.Username,
                    Power = player.Power,
                    Wins = player.Wins,
                    Losses = player.Losses
                }
            };
        }

        public async Task<Player?> GetPlayerById(int id)
        {
            return await _context.Players.FindAsync(id);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hash = HashPassword(password);
            return hash == hashedPassword;
        }

        private string GenerateJwtToken(Player player)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyMinimum32Characters!!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ShutTheTwelveAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "ShutTheTwelveClient";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                new Claim(ClaimTypes.Name, player.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}