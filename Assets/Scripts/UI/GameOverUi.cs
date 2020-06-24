using Level;
using Score;
using TMPro;
using UnityEngine;

namespace UI
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
