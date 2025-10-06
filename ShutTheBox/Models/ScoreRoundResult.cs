namespace ShutTheBox.Models
{
    public class ScoreRoundResult
    {
        public int Player1RoundScore { get; set; }
        public int Player2RoundScore { get; set; }
        public int Player1TotalScore { get; set; }
        public int Player2TotalScore { get; set; }
        public int CurrentRound { get; set; }
        public bool IsLastRound { get; set; }
    }
}
