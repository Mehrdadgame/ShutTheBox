using ShutTheBox.Models;
using ShutTheTwelveBackend.Models;

namespace ShutTheTwelve.Backend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<Player?> GetPlayerById(int id);
    }
}