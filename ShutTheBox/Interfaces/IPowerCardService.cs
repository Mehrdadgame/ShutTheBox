using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Models;

namespace ShutTheTwelve.Backend.Interfaces
{
    public interface IPowerCardService
    {
        Task<List<PowerCard>> GetAllCards();
        Task<PowerCard?> GetCardByType(string effectType);
    }
}