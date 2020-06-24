using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace Score
{
    public static class HighScoreManager
    {
        public static string PersistencePath => Path.Combine(Application.dataPath, "Resources", "scores.json");
        private static List<LevelScore> _levelScores;

        public static IEnumerable<LevelScore> LevelScores
        {
            get
            {
                if (_levelScores == null) _levelScores = ReadScores();

                return _levelScores.AsReadOnly();
            }
        }

        public static void Write()
        {
            if (_levelScores is null)
            {
                Debug.LogError("Level Scores not initialized");
                return;
            }

            using (var sw = new StreamWriter(PersistencePath, false))
            {
                var json = JsonConvert.SerializeObject(_levelScores, Formatting.Indented);
                sw.Write(json);
            }
        }

        public static List<LevelScore> ReadScores(bool force = false)
        {
            if (!force && _levelScores != null) return _levelScores;

            using (var reader = new StreamReader(PersistencePath))
            {
                return JsonConvert.DeserializeObject<List<LevelScore>>(reader.ReadToEnd()) ??
                               new List<LevelScore>();
            }
        }

        public static void InitializeLevel(string levelName, bool write=true)
        {
            Debug.Log($"Initializing {levelName}");
            ReadScores();
            if (LevelScores.All(score => score.LevelName != levelName))
            {
                _levelScores.Add(new LevelScore(levelName, null));
            }

            if (write) Write();
        }

        public static void AddNewUserScore(string levelName, UserScore userScore)
        {
            if (string.IsNullOrWhiteSpace(levelName)) throw new ArgumentException("Level name can not be empty", nameof(levelName));
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
