using EventArgs;
using Level;
using Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ScoreUpdater : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private Image _icon;

        private void Awake()
        {
            _scoreText = GetComponent<TextMeshProUGUI>();
            _icon = transform.GetChild(0).GetComponent<Image>();

            _scoreText.enabled = true;
            _icon.enabled = true;
            _scoreText.text = "0";
        }

        private void OnEnable()
        {
            ScoreManager.ScoreUpdated += OnScoreUpdated;
            LevelManager.GameOver += LevelManagerOnGameOver;
        }

        private void OnDisable()
        {
            ScoreManager.ScoreUpdated -= OnScoreUpdated;
            LevelManager.GameOver -= LevelManagerOnGameOver;
        }

        private void LevelManagerOnGameOver(object sender, System.EventArgs e)
        {
            _scoreText.enabled = false;
            _icon.enabled = false;
        }

        private void OnScoreUpdated(object sender, ScoreUpdatedEventArgs e)
        {
            _scoreText.text = $"{e.NewScore}";
        }
    }
}
