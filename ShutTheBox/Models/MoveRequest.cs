namespace ShutTheBox.Models
{
    public class MoveRequest
    {
        public int GameId { get; set; }
        public List<int> SelectedNumbers { get; set; } = new();
    }
}
