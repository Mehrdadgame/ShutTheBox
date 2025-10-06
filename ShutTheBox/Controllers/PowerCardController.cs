using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutTheTwelve.Backend.Interfaces;
using ShutTheTwelve.Backend.Models;

namespace ShutTheTwelveBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PowerCardController : ControllerBase
    {
        private readonly IPowerCardService _powerCardService;

        public PowerCardController(IPowerCardService powerCardService)
        {
            _powerCardService = powerCardService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<PowerCard>>> GetAllCards()
        {
            var cards = await _powerCardService.GetAllCards();
            return Ok(cards);
        }

        [HttpGet("{effectType}")]
        public async Task<ActionResult<PowerCard>> GetCardByType(string effectType)
        {
            var card = await _powerCardService.GetCardByType(effectType);

            if (card == null)
                return NotFound(new { message = "Card not found" });

            return Ok(card);
        }
    }
}