using System;
using Phoenix.EventArgs;
using Phoenix.Level;
using UnityEngine;

namespace Phoenix.Score
{
    public class ScoreManager : MonoBehaviour
    {
        private static ScoreManager _instance;
        private int _score;


        public static int Score => _instance._score;


        public static event EventHandler<ScoreUpdatedEventArgs> ScoreUpdated = delegate { };

        private void Awake()
        {
            if (_instance != null) Debug.LogWarning("Overriding");
            _instance = this;
        }

        public static void AddScore(int scoreToAdd)
        {
            if (scoreToAdd < 0) throw new ArgumentException("Score can not be negative", nameof(scoreToAdd));

            if (LevelManager.IsGameOver)
            {
                Debug.LogWarning("Game is already over");
                return;
            }

            var oldScore = _instance._score;
            _instance._score += scoreToAdd;

            ScoreUpdated(_instance, new ScoreUpdatedEventArgs(_instance._score, oldScore));
        }
    }
}
