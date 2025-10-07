using System.ComponentModel.DataAnnotations;

namespace ShutTheTwelveServer.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string ClientVersion { get; set; } = "1.0.0";
    }

    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string ClientVersion { get; set; } = "1.0.0";
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public bool RequiresUpdate { get; set; }
        public string LatestVersion { get; set; }
    }
}