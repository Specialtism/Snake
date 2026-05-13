using Snake.Enums;

namespace Snake.Core
{
    public class GameSettings
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public GameMode Mode { get; set; }
        public int StartingSpeed { get; set; }
        public int SpeedBoostDuration {  get; set; }
        public int SpeedBoostSpeed { get; set; }


        public static GameSettings Classic => new()
        {
            Rows = 25,
            Cols = 25,
            Mode = GameMode.Classic,
            StartingSpeed = 100,

            SpeedBoostDuration = 3000,
            SpeedBoostSpeed = 75
        };

        public static GameSettings LargeGrid => new()
        {
            Rows = 50,
            Cols = 50,
            Mode = GameMode.LargeGrid,
            StartingSpeed = 100,

            SpeedBoostDuration = 5000,
            SpeedBoostSpeed = 30
        };
    };
}
