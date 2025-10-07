using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShutTheTwelveServer.Data;
using ShutTheTwelveServer.DTOs;
using ShutTheTwelveServer.Models;

namespace ShutTheTwelveServer.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> Register(RegisterDTO dto);
        Task<AuthResponseDTO> Login(LoginDTO dto);
        string GenerateJwtToken(Player player);
        bool CheckVersion(string clientVersion);
    }

    public class AuthService : IAuthService
    {
        private readonly GameDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _currentVersion = "1.0.0";

        public AuthService(GameDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDTO> Register(RegisterDTO dto)
        {
            // Check if user exists
            var existingUser = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == dto.Username || p.Email == dto.Email);

            if (existingUser != null)
            {
                throw new Exception("Username or email already exists");
            }

            // Create new player
            var player = new Player
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                ClientVersion = dto.ClientVersion,
                CreatedAt = DateTime.UtcNow
            };

            // Add default cards for new player
            var defaultCards = await _context.PowerCards
                .Where(c => c.Rarity == CardRarity.Common)
                .ToListAsync();

            foreach (var card in defaultCards)
            {
                player.PlayerCards.Add(new PlayerCard
                {
                    PlayerId = player.Id,
                    PowerCardId = card.Id,
                    Quantity = 1
                });
            }

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(player);
            var requiresUpdate = !CheckVersion(dto.ClientVersion);

            return new AuthResponseDTO
            {
                Token = token,
                Username = player.Username,
                Level = player.Level,
                Experience = player.Experience,
                RequiresUpdate = requiresUpdate,
                LatestVersion = _currentVersion
            };
        }

        public async Task<AuthResponseDTO> Login(LoginDTO dto)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == dto.Username);

            if (player == null || !BCrypt.Net.BCrypt.Verify(dto.Password, player.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            player.LastLoginAt = DateTime.UtcNow;
            player.ClientVersion = dto.ClientVersion;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(player);
            var requiresUpdate = !CheckVersion(dto.ClientVersion);

            return new AuthResponseDTO
            {
                Token = token,
                Username = player.Username,
                Level = player.Level,
                Experience = player.Experience,
                RequiresUpdate = requiresUpdate,
                LatestVersion = _currentVersion
            };
        }

        public string GenerateJwtToken(Player player)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, player.Id.ToString()),
                    new Claim(ClaimTypes.Name, player.Username),
                    new Claim(ClaimTypes.Email, player.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool CheckVersion(string clientVersion)
        {
            return clientVersion == _currentVersion;
        }
    }
}