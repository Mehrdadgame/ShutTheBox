namespace ShutTheBox.Services
{
    public class MatchmakingResult
    {
        public bool MatchFound { get; set; }
        public int? GameSessionId { get; set; }
        public bool IsAIMatch { get; set; }
        public int? OpponentId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
