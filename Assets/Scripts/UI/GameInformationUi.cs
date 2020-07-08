using Phoenix.EventArgs;
using Phoenix.Level;
using Phoenix.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Phoenix.UI
{
    public class GameInformationUi : MonoBehaviour
    {
        public GameObject GameOverContent;
        public TextMeshProUGUI GameStartDelayText;
        public TextMeshProUGUI GameOverScoreText;
        public TextMeshProUGUI TimeText;
        public TextMeshProUGUI ScoreText;
        public RawImage ScoreBoxRawImage;


        private void OnEnable()
        {
            LevelManager.GameStarting += OnGameStarting;
            LevelManager.GameStarted += OnGameStarted;
            LevelManager.GameStarterTimeUpdated += OnGameStarterTimeUpdated;
            LevelManager.GameOver += OnGameOver;
        }

        private void OnDisable()
        {
            LevelManager.GameStarting -= OnGameStarting;
            LevelManager.GameStarted -= OnGameStarted;
            LevelManager.GameStarterTimeUpdated -= OnGameStarterTimeUpdated;
            LevelManager.GameOver -= OnGameOver;
        }
        
        private void OnGameStarting(object sender, System.EventArgs e)
        {
            GameStartDelayText.gameObject.SetActive(true);
        }

        private void OnGameStarted(object sender, System.EventArgs e)
        {
            GameStartDelayText.gameObject.SetActive(false);
            TimeText.gameObject.SetActive(true);
            ScoreText.gameObject.SetActive(true);
            ScoreBoxRawImage.gameObject.SetActive(true);
        }

        private void OnGameStarterTimeUpdated(object sender, TimeUpdatedEventArgs e)
        {
            GameStartDelayText.text = $"{e.Time:0}";
        }
        
        private void OnGameOver(object sender, System.EventArgs e)
        {
            TimeText.gameObject.SetActive(false);
            ScoreText.gameObject.SetActive(false);
            ScoreBoxRawImage.gameObject.SetActive(false);
            GameOverContent.SetActive(true);
            GameOverScoreText.text = $"{ScoreManager.Score}";
        }
    }
}

