using Microsoft.EntityFrameworkCore;
using ShutTheTwelve.Backend.Interfaces;
using ShutTheTwelve.Backend.Models;
using ShutTheTwelveBackend.Data;

namespace ShutTheTwelve.Backend.Services
{
    public class PowerCardService : IPowerCardService
    {
        private readonly GameDbContext _context;

        public PowerCardService(GameDbContext context)
        {
            _context = context;
        }

        public async Task<List<PowerCard>> GetAllCards()
        {
            return await _context.PowerCards
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<PowerCard?> GetCardByType(string effectType)
        {
            return await _context.PowerCards
                .FirstOrDefaultAsync(c => c.EffectType == effectType && c.IsActive);
        }
    }
}