namespace ShutTheTwelve.Backend.Enums
{
    public enum GameMode
    {
        Classic,
        Score
    }

    public enum PlayerTurn
    {
        Player1,
        Player2
    }

    public enum RollPhase
    {
        NotRolled,
        Rolled,
        Processing
    }

    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        Finished,
        Abandoned
    }

    public enum CardType
    {
        LockAndReroll,
        Sabotage,
        SecondChance,
        WildDice,
        StealTurn,
        Shield,
        LightningRoll,
        SwapBoard,
        Mimic
    }

    public enum CardTiming
    {
        BeforeRoll,
        AfterRoll,
        AfterOpponentRoll,
        Reactive,
        Anytime,
        NoMoves
    }
    public enum GamePhase
    {
        BeforeRoll,      // قبل از تاس انداختن
        AfterRoll,       // بعد از تاس انداختن
        AfterOpponentRoll, // بعد از تاس حریف
        BeforeSelection,  // قبل از انتخاب عدد
        Reactive,        // واکنشی (برای Shield)
        Anytime          // هر زمان
    }

    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}