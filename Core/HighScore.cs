using System;
using Snake.Enums;

namespace Snake.Core
{
    public class HighScore
    {
        public int Score { get; set; }
        public DateTime DateAchieved { get; set; }
        public GameMode Mode { get; set; } // <-- Add this
    }
}
