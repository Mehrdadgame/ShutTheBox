namespace ShutTheBox.DTOs
{
    public class GameStateDto
    {
        public int GameId { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ActiveTurn { get; set; } = string.Empty;
        public bool IsAIGame { get; set; }

        public List<int> Player1Board { get; set; } = new();
        public List<int> Player2Board { get; set; } = new();
        public List<string> Player1Cards { get; set; } = new();
        public List<string> Player2Cards { get; set; } = new();

        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }

        public int? WinnerId { get; set; }
    }

}
