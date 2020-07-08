using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Phoenix.Level;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace Phoenix.Score
{
    public static class HighScoreManager
    {
        private const string LevelDataLocation = "Levels";
        public static string PersistencePath => Path.Combine(Application.dataPath, "Resources", "scores.json");
        private static readonly List<LevelScore> _levelScores;

        static HighScoreManager()
        {
            if (File.Exists(PersistencePath))
            {
                _levelScores = ReadScores();
            }
            else
            {
                _levelScores = new List<LevelScore>();
                var levelData = Resources.LoadAll<LevelData>(LevelDataLocation).ToList();
                levelData.ForEach(data => _levelScores.Add(new LevelScore(data.levelName, new List<UserScore>())));
                Write();
            }
        }

        public static IEnumerable<LevelScore> LevelScores => _levelScores.AsReadOnly();

        public static void Write()
        {
            if (_levelScores is null)
            {
                Debug.LogError("Level Scores not initialized");
                return;
            }

            using (var sw = File.CreateText(PersistencePath))
            {
                var json = JsonConvert.SerializeObject(_levelScores, Formatting.Indented);
                sw.Write(json);
            }
        }

        public static List<LevelScore> ReadScores()
        {
            using (var reader = new StreamReader(PersistencePath))
            {
                return JsonConvert.DeserializeObject<List<LevelScore>>(reader.ReadToEnd()) ??
                       new List<LevelScore>();
            }
        }

        public static void AddNewUserScore(string levelName, UserScore userScore)
        {
            if (string.IsNullOrWhiteSpace(levelName))
                throw new ArgumentException("Level name can not be empty", nameof(levelName));
            var levelScore = LevelScores.Single(s => s.LevelName == levelName);
            if (levelScore is null)
            {
                Debug.LogError("Level Scores json not initialized or level not found");
                return;
            }

            levelScore.AddScore(userScore);
        }
    }
}