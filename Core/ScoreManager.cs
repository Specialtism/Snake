using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Snake.Enums;

namespace Snake.Core
{
	public static class ScoreManager
	{
		private const string FilePath = "highscores.json";

		public static List<HighScore> LoadHighScores(GameMode mode) // Pass mode in here
		{
			if (!File.Exists(FilePath))
				return new List<HighScore>();

			try
			{
				string json = File.ReadAllText(FilePath);
				var allScores = JsonSerializer.Deserialize<List<HighScore>>(json) ?? new List<HighScore>();

				// Only return scores that match the requested mode
				return allScores.Where(s => s.Mode == mode).ToList();
			}
			catch
			{
				return new List<HighScore>();
			}
		}

		public static void SaveScore(int newScore, GameMode mode)
		{
			// Important: We need to load ALL scores to save properly, not just the filtered ones!
			List<HighScore> allScores = new List<HighScore>();
			if (File.Exists(FilePath))
			{
				try
				{
					string existingJson = File.ReadAllText(FilePath);
					allScores = JsonSerializer.Deserialize<List<HighScore>>(existingJson) ?? new List<HighScore>();
				}
				catch { }
			}

			allScores.Add(new HighScore
			{
				Score = newScore,
				DateAchieved = DateTime.Now,
				Mode = mode // Assign the mode
			});

			// Group the scores by mode, sort each mode's top 10, then put them back together
			var topScores = allScores
				.GroupBy(s => s.Mode)
				.SelectMany(group => group.OrderByDescending(s => s.Score).Take(3))
				.ToList();

			string json = JsonSerializer.Serialize(topScores, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(FilePath, json);
		}
	}
}