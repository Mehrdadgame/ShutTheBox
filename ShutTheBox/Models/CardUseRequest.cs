namespace ShutTheBox.Models
{
    public class CardUseRequest
    {
        public int GameId { get; set; }
        public string CardType { get; set; } = string.Empty;
        public int? TargetDie { get; set; } // For LockAndReroll
    }
}
