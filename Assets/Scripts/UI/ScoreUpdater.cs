using Phoenix.EventArgs;
using Phoenix.Score;
using TMPro;
using UnityEngine;

namespace Phoenix.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ScoreUpdater : MonoBehaviour
    {
        public TextMeshProUGUI ScoreText;

        void Start()
        {
            ScoreText = GetComponent<TextMeshProUGUI>();
            ScoreManager.ScoreUpdated += OnScoreUpdated;
            ScoreText.text = "0";
        }

        private void OnScoreUpdated(object sender, ScoreUpdatedEventArgs e)
        {
            ScoreText.text = $"{e.NewScore}";
        }
    }
}
