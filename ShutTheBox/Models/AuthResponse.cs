using ShutTheBox.DTOs;

namespace ShutTheBox.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public PlayerDto? Player { get; set; }
    }
}
