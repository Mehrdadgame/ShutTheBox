namespace ShutTheBox.Models
{
    public class GameEndResult
    {
        public int? WinnerId { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
