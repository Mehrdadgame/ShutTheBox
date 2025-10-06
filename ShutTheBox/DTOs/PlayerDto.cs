namespace ShutTheBox.DTOs
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Power { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}
