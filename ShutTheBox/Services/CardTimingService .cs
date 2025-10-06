using ShutTheBox.Interfaces;
using ShutTheTwelve.Backend.Enums;

namespace ShutTheBox.Services
{
    public class CardTimingService : ICardTimingService
    {
        public bool CanUseCard(string cardType, GamePhase currentPhase, bool isPlayerTurn)
        {
            var requiredPhase = GetCardRequiredPhase(cardType);

            switch (requiredPhase)
            {
                case GamePhase.BeforeRoll:
                    return currentPhase == GamePhase.BeforeRoll && isPlayerTurn;

                case GamePhase.AfterRoll:
                    return currentPhase == GamePhase.AfterRoll && isPlayerTurn;

                case GamePhase.AfterOpponentRoll:
                    return currentPhase == GamePhase.AfterOpponentRoll && !isPlayerTurn;

                case GamePhase.Reactive:
                    return currentPhase == GamePhase.Reactive;

                case GamePhase.Anytime:
                    return isPlayerTurn;

                default:
                    return false;
            }
        }

        public GamePhase GetCardRequiredPhase(string cardType)
        {
            return cardType.ToLower() switch
            {
                "lockreroll" or "lockandreroll" => GamePhase.AfterRoll,
                "secondchance" => GamePhase.AfterRoll,

                "sabotage" => GamePhase.BeforeRoll,
                "wilddie" or "wilddice" => GamePhase.BeforeRoll,
                "lightningroll" => GamePhase.BeforeRoll,

                "stealturn" => GamePhase.AfterOpponentRoll,

                "shield" => GamePhase.Reactive,

                "swapboard" => GamePhase.Anytime,
                "mimic" => GamePhase.Anytime,

                _ => GamePhase.Anytime
            };
        }

        public string GetTimingMessage(string cardType)
        {
            var phase = GetCardRequiredPhase(cardType);

            return phase switch
            {
                GamePhase.BeforeRoll => "این کارت قبل از تاس انداختن استفاده می‌شود",
                GamePhase.AfterRoll => "این کارت بعد از تاس انداختن استفاده می‌شود",
                GamePhase.AfterOpponentRoll => "این کارت بعد از تاس حریف استفاده می‌شود",
                GamePhase.Reactive => "این کارت فقط در واکنش به کارت حریف استفاده می‌شود",
                GamePhase.Anytime => "این کارت در هر زمان قابل استفاده است",
                _ => "زمان نامشخص"
            };
        }

        // کارت‌هایی که بعد از Roll حریف قابل استفاده هستند
        public List<string> GetCardsUsableAfterOpponentRoll()
        {
            return new List<string> { "StealTurn" };
        }

        // کارت‌های تهاجمی که Shield می‌تواند بلاک کند
        public List<string> GetBlockableCards()
        {
            return new List<string>
            {
                "Sabotage",
                "StealTurn",
                "LightningRoll",
                "SwapBoard"
            };
        }

        // کارت‌هایی که Mimic می‌تواند کپی کند
        public List<string> GetMimicableCards()
        {
            return new List<string>
            {
                "LockReroll",
                "Sabotage",
                "WildDie",
                "LightningRoll",
                "SwapBoard"
                // Shield و SecondChance قابل کپی نیستند
            };
        }
    }
}

