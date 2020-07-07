using Phoenix.Score;
using TMPro;
using UnityEngine;

namespace Phoenix.UI
{
    public class GameOverUi : MonoBehaviour
    {
        public TextMeshProUGUI ScoreText;
        public HighScoreListUi HighScoreList;

        public void OnGameOver(object sender, System.EventArgs e)
        {
            ScoreText.text = $"Your Score: {ScoreManager.Score}";
            HighScoreList.SetScoresForCurrentLevel();
        }
    }
}
