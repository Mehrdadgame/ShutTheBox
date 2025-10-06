using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutTheBox.Interfaces;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Interfaces;
using ShutTheTwelve.Backend.Models;

namespace ShutTheBox.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PowerCardController : ControllerBase
    {
        private readonly IPowerCardService _powerCardService;
        private readonly ICardTimingService _cardTimingService;

        public PowerCardController(IPowerCardService powerCardService, ICardTimingService cardTimingService)
        {
            _powerCardService = powerCardService;
            _cardTimingService = cardTimingService;
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

        [HttpGet("timing/{cardType}")]
        public ActionResult GetCardTiming(string cardType)
        {
            var phase = _cardTimingService.GetCardRequiredPhase(cardType);
            var message = _cardTimingService.GetTimingMessage(cardType);

            return Ok(new
            {
                cardType,
                requiredPhase = phase.ToString(),
                message
            });
        }

        [HttpGet("can-use")]
        public ActionResult CanUseCard([FromQuery] string cardType, [FromQuery] string currentPhase, [FromQuery] bool isPlayerTurn)
        {
            if (!Enum.TryParse<GamePhase>(currentPhase, out var phase))
            {
                return BadRequest(new { message = "Invalid game phase" });
            }

            bool canUse = _cardTimingService.CanUseCard(cardType, phase, isPlayerTurn);

            return Ok(new
            {
                canUse,
                message = canUse ? "Card can be used" : _cardTimingService.GetTimingMessage(cardType)
            });
        }

        [HttpGet("blockable")]
        public ActionResult GetBlockableCards()
        {
            var cards = _cardTimingService.GetBlockableCards();
            return Ok(cards);
        }

        [HttpGet("mimicable")]
        public ActionResult GetMimicableCards()
        {
            var cards = _cardTimingService.GetMimicableCards();
            return Ok(cards);
        }
    }
}