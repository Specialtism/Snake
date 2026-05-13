using Snake.Enums;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace Snake.Core;

public class GameState
{
    public int Rows { get; }
    public int Col { get; }
    public GridValue[,] Grid { get; }
    public Direction Dir { get; private set; }
    public int Score { get; private set; }
    public bool GameOver { get; private set; }

    public int CurrentSpeed { get; private set; }
    private readonly GameSettings _settings;

    public bool IsSpeedBoostActive { get; private set; }

    private readonly LinkedList<Direction> dirChanges = new();
    private readonly LinkedList<Position> snakePositions = new();
    private readonly Random random = new();

    public GameState(GameSettings settings)
    {
        _settings = settings;
        Rows = settings.Rows;
        Col = settings.Cols;
        CurrentSpeed = settings.StartingSpeed; 

        Grid = new GridValue[Rows, Col];
        Dir = Direction.Right;

        AddSnake();
        AddFood();
    }

    private void AddSnake()
    {
        int r = Rows / 2;

        for (int c = 1; c <= 3; c++)
        {
            Grid[r, c] = GridValue.Snake;
            snakePositions.AddFirst(new Position(r, c));
        }
    }

    private IEnumerable<Position> EmptyPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Col; c++)
            {
                if (Grid[r, c] == GridValue.Empty)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    private void AddFood()
    {
        List<Position> empty = [.. EmptyPositions()];

        if (empty.Count == 0)
        {
            return;
        }

        Position pos = empty[random.Next(empty.Count)];

        if (_settings.Mode == GameMode.Classic || _settings.Mode == GameMode.ClassicHard)
        {
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }
        else if (_settings.Mode == GameMode.LargeGrid)
        {
            // 60% standard food, 20% speed up, 20% slow down
            int roll = random.Next(0, 100);
            if (roll < 60)
                Grid[pos.Row, pos.Col] = GridValue.Food;
            else if (roll < 80)
                Grid[pos.Row, pos.Col] = GridValue.FoodSpeedUp;
            else
                Grid[pos.Row, pos.Col] = GridValue.FoodSlowDown;
        }
    }

    public Position HeadPosition()
    {
        return snakePositions.First.Value;
    }

    public Position TailPosition()
    {
        return snakePositions.Last.Value;
    }

    public IEnumerable<Position> SnakePositions()
    {
        return snakePositions;
    }

    private void AddHead(Position pos)
    {
        snakePositions.AddFirst(pos);
        Grid[pos.Row, pos.Col] = GridValue.Snake;
    }

    private void RemoveTail()
    {
        Position tail = snakePositions.Last.Value;
        Grid[tail.Row, tail.Col] = GridValue.Empty;
        snakePositions.RemoveLast();
    }

    private Direction GetLastDirection()
    {
        if (dirChanges.Count == 0)
        {
            return Dir;
        }

        return dirChanges.Last.Value;
    }

    private bool CanChangeDirection(Direction newDir)
    {
        if (dirChanges.Count == 2)
        {
            return false;
        }

        Direction lastDir = GetLastDirection();
        return newDir != lastDir && newDir != lastDir.Opposite();
    }

    public void ChangeDirection(Direction dir)
    {
        if (CanChangeDirection(dir))
        {
            dirChanges.AddLast(dir);
        }
    }

    private bool OutsideGrid(Position pos)
    {
        return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Col;
    }

    private GridValue WillHit(Position newHeadPos)
    {
        if (OutsideGrid(newHeadPos))
        {
            return GridValue.Outside;
        }

        if (newHeadPos == TailPosition())
        {
            return GridValue.Empty;
        }
        return Grid[newHeadPos.Row, newHeadPos.Col];
    }
    private void AddSpeedBoostItem()
    {
        List<Position> empty = new List<Position>(EmptyPositions());

        if (empty.Count == 0)
        {
            return;
        }

        // Pick a random empty spot
        Position pos = empty[random.Next(empty.Count)];

        // Ensure you have added SpeedBoost to your GridValue enum!
        Grid[pos.Row, pos.Col] = GridValue.SpeedBoost;
    }
    private async void StartTimeBasedSpeedBoost(int durationInMilliseconds)
    {
        IsSpeedBoostActive = true;
        OnSpeedBoostConsumed?.Invoke(); // Trigger the animation

        // Wait for the specified amount of time (e.g., 5000ms = 5 seconds)
        await Task.Delay(durationInMilliseconds);

        IsSpeedBoostActive = false;
    }

    public void Move()
    {
        if (dirChanges.Count > 0)
        {
            Dir = dirChanges.First.Value;
            dirChanges.RemoveFirst();
        }
        Position newHeadPos = HeadPosition().Translate(Dir);
        GridValue hit = WillHit(newHeadPos);

        if (hit == GridValue.Outside || hit == GridValue.Snake)
        {
            GameOver = true;
        }
        else if (hit == GridValue.Empty)
        {
            RemoveTail();
            AddHead(newHeadPos);
        }
        // Check for all 3 food types
        else if (hit == GridValue.Food || hit == GridValue.FoodSpeedUp || hit == GridValue.FoodSlowDown)
        {
            AddHead(newHeadPos);
            Score++;

            if (_settings.Mode != GameMode.Classic)
            {
                // Rolls for chance to spawn Speed boost
                if (random.NextDouble() < 0.10)
                {
                    AddSpeedBoostItem();
                }
            }
            // Adjust speed dynamically based on the food type eaten
            if (hit == GridValue.FoodSpeedUp)
            {
                // Lower delay means faster game. Prevent it from going impossibly fast (e.g., minimum 20)
                CurrentSpeed = Math.Max(20, CurrentSpeed - 10);
            }
            else if (hit == GridValue.FoodSlowDown)
            {
                CurrentSpeed += 10;
            }

            AddFood();

        }

        else if (hit == GridValue.SpeedBoost)
        {
            RemoveTail();
            AddHead(newHeadPos);

            // Start a 5-second timer
            StartTimeBasedSpeedBoost(_settings.SpeedBoostDuration);
        }

    }
    public event Action OnSpeedBoostConsumed;
}
