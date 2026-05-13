using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Resources;
using Snake.Core;
using Snake.Enums;

namespace Snake
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food },
            { GridValue.FoodSpeedUp, Images.Food },
            { GridValue.FoodSlowDown, Images.Food },
            { GridValue.SpeedBoost, Images.SpeedBoost },

        };

        private readonly Dictionary<Direction, int> dirToRotation = new()
        {
            {Direction.Up, 0 },
            {Direction.Right, 90 },
            {Direction.Down, 180 },
            {Direction.Left, 270 },
        };

        private Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private GameSettings currentSettings;

        public MainWindow()
        {
            InitializeComponent();
            Overlay.Visibility = Visibility.Hidden;
            UpdateLeaderboardUI();
        }

        private async void BtnClassic_Click(object sender, RoutedEventArgs e)
        {
            if (ChkHardMode.IsChecked == true)
            {
                // You can duplicate the GameSettings.Classic preset and name it ClassicHard, 
                // but change the Mode = GameMode.ClassicHard!
                GameSettings hardSettings = GameSettings.Classic;
                hardSettings.Mode = GameMode.ClassicHard;

                await StartNewGame(hardSettings);
            }
            else
            {
                await StartNewGame(GameSettings.Classic);
            }
        }

        private async void BtnLargeGrid_Click(object sender, RoutedEventArgs e)
        {
            await StartNewGame(GameSettings.LargeGrid);
        }

        private async Task StartNewGame(GameSettings settings)
        {
            currentSettings = settings;

            // Hide Main Menu
            MainMenuOverlay.Visibility = Visibility.Hidden;
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "3";

            // Generate State & UI Grid
            gameState = new GameState(currentSettings);
            gameState.OnSpeedBoostConsumed += GameState_OnSpeedBoostConsumed;
            gridImages = SetupGrid(currentSettings.Rows, currentSettings.Cols);

            // Start the game loop
            gameRunning = true;
            await RunGame();
            gameRunning = false;
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState.OnSpeedBoostConsumed -= GameState_OnSpeedBoostConsumed;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block input while countdown is happening
            if (Overlay.Visibility == Visibility.Visible && OverlayText.Text != "PRESS ANY KEY TO RETURN TO MENU")
            {
                e.Handled = true;
            }

            // Return to menu if game is over and they press a key
            if (Overlay.Visibility == Visibility.Visible && OverlayText.Text == "PRESS ANY KEY TO RETURN TO MENU")
            {
                e.Handled = true;
                Overlay.Visibility = Visibility.Hidden;

                UpdateLeaderboardUI();

                MainMenuOverlay.Visibility = Visibility.Visible; // Show menu again
            }
        }

        private void UpdateLeaderboardUI()
        {
            var classic = ScoreManager.LoadHighScores(GameMode.Classic);
            var hard = ScoreManager.LoadHighScores(GameMode.ClassicHard);
            var large = ScoreManager.LoadHighScores(GameMode.LargeGrid);

            LeaderboardText.Text =
                "TOP CLASSIC\n" + FormatScores(classic) + "\n\n" +
                "TOP HARD MODE\n" + FormatScores(hard) + "\n\n" +
                "TOP LARGE GRID\n" + FormatScores(large);
        }

        // Helper method to keep it clean
        private string FormatScores(List<HighScore> scores)
        {
            if (scores.Count == 0) return "No Scores!";
            string result = "";

            // Change this bounds calculation to take the smaller of the two numbers
            int count = Math.Min(scores.Count, 3);

            for (int i = 0; i < count; i++)
            {
                result += $"{i + 1}. {scores[i].Score} pts ({scores[i].DateAchieved:MM/dd})\n";
            }
            return result;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Stops user input when game over
            if(gameState.GameOver)
            {
                return;
            }
            // user input
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left); break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right); break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up); break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down); break;
            }
        }

        // change delay to change speed of movement
        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                int currentDelay = gameState.IsSpeedBoostActive ? currentSettings.SpeedBoostSpeed: gameState.CurrentSpeed;
                await Task.Delay(currentDelay);
                gameState.Move();
                Draw();
            }
        }

        private Image[,] SetupGrid(int rows, int cols)
        {
            // Clear old children if we just finished a previous game
            GameGrid.Children.Clear();

            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text =$"SCORE {gameState.Score}";
            // Show the speed boost speed if active, otherwise show the normal speed (delay)
            int displaySpeed = gameState.IsSpeedBoostActive ? currentSettings.SpeedBoostSpeed : gameState.CurrentSpeed;
            SpeedText.Text = $"DELAY {displaySpeed}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < gameState.Rows; r++)
            {
                for (int c = 0; c < gameState.Col; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = [.. gameState.SnakePositions()];

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);

            // --> SAVE SCORE HERE <--
            ScoreManager.SaveScore(gameState.Score, currentSettings.Mode);

            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO RETURN TO MENU";
        }
        private void GameState_OnSpeedBoostConsumed()
        {
            // Find the storyboard from MainWindow.xaml resources
            System.Windows.Media.Animation.Storyboard speedAnimation =
                (System.Windows.Media.Animation.Storyboard)this.FindResource("SpeedBoostAnimation");

            // Grab the first animation inside the storyboard (the DoubleAnimation)
            var doubleAnim = (System.Windows.Media.Animation.DoubleAnimation)speedAnimation.Children[0];

            // Calculate how many seconds the duration is (e.g., 5000ms / 1000 = 5 seconds)
            int repeatCount = currentSettings.SpeedBoostDuration / 1000;

            // Set the repeat behavior to match the seconds
            doubleAnim.RepeatBehavior = new System.Windows.Media.Animation.RepeatBehavior(repeatCount);

            // Play the animation
            speedAnimation.Begin();
        }
    }
}