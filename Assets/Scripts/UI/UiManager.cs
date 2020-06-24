using EventArgs;
using Level;
using Score;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UiManager : MonoBehaviour
    {
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI TimeText;

        private void Start()
        {
            ScoreManager.ScoreUpdated += OnScoreUpdated;
            LevelManager.TimeUpdated += OnTimeUpdated;

            TimeText.text = $"Time: {LevelManager.GameDuration:F1}/{LevelManager.GameDuration:F1}";
        }

        private void OnTimeUpdated(object sender, TimeUpdatedEventArgs e)
        {
            TimeText.text = $"Time: {e.TotalTime - e.Time:F1}/{e.TotalTime:F1}";
        }

        private void OnScoreUpdated(object sender, ScoreUpdatedEventArgs e)
        {
            ScoreText.text = $"Your Score: {e.NewScore}";
        }
    }
}
