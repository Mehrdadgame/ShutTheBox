using ShutTheTwelve.Backend.Enums;

namespace ShutTheBox.Interfaces
{
    public interface ICardTimingService
    {
        bool CanUseCard(string cardType, GamePhase currentPhase, bool isPlayerTurn);
        GamePhase GetCardRequiredPhase(string cardType);
        string GetTimingMessage(string cardType);
    }
}
