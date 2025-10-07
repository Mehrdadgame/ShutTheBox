namespace ShutTheTwelveServer.DTOs
{
    public class LeaderboardEntryDTO
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public int Score { get; set; }
        public int Level { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public float WinRate { get; set; }
    }

    public class LeaderboardDTO
    {
        public string Type { get; set; } // "Weekly" or "Monthly"
        public List<LeaderboardEntryDTO> Entries { get; set; }
        public LeaderboardEntryDTO PlayerEntry { get; set; }
        public DateTime LastReset { get; set; }
        public DateTime NextReset { get; set; }
    }
}