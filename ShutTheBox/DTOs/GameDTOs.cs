using System;
using System.Collections.Generic;

namespace ShutTheTwelveServer.DTOs
{
    public class GameStateDTO
    {
        public Guid SessionId { get; set; }
        public string GameMode { get; set; }
        public string CurrentTurn { get; set; }
        public List<int> PlayerBoard { get; set; }
        public List<int> OpponentBoard { get; set; }
        public int LastDice1 { get; set; }
        public int LastDice2 { get; set; }
        public int PlayerScore { get; set; }
        public int OpponentScore { get; set; }
        public int CurrentRound { get; set; }
        public bool IsAgainstBot { get; set; }
        public string OpponentName { get; set; }
    }

    public class DiceRollDTO
    {
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }
        public bool IsSabotaged { get; set; }
        public int? WildDieValue { get; set; }
    }

    public class MoveDTO
    {
        public List<int> SelectedNumbers { get; set; }
        public Guid SessionId { get; set; }
    }

    public class UseCardDTO
    {
        public int CardId { get; set; }
        public Guid SessionId { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class MatchmakingRequestDTO
    {
        public string GameMode { get; set; }
        public List<int> SelectedCards { get; set; }
    }
}