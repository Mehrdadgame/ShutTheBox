namespace ShutTheBox.Services
{
    public class MatchmakingRequest
    {
        public int PlayerId { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public DateTime QueueTime { get; set; } = DateTime.UtcNow;
    }
}
