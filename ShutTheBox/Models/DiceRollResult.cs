namespace ShutTheBox.Models
{
    public class DiceRollResult
    {
        public int Die1 { get; set; }
        public int Die2 { get; set; }
        public string PlayerTurn { get; set; } = string.Empty;
    }
}
