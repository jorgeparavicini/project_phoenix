using Score;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UserScoreUi : MonoBehaviour
    {
        private UserScore _score;
        public UserScore Score
        {
            get => _score;
            set
            {
                _score = value; 
                UpdateUi();
            }
        }

        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ScoreText;

        private void UpdateUi()
        {
            NameText.text = Score.UserName;
            ScoreText.text = $"{Score.Score:D}";
        }
    }
}
