using Phoenix.Level;
using Phoenix.Score;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phoenix.UI
{
    public class SaveScoreUi : MonoBehaviour
    {
        public TMP_InputField UserName;
        public TextMeshProUGUI ErrorText;

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(UserName.text))
            {
                ErrorText.text = "Please enter a valid username";
                ErrorText.gameObject.SetActive(true);
                return;
            }

            HighScoreManager.AddNewUserScore(LevelManager.LevelName, new UserScore(UserName.text, ScoreManager.Score));
            HighScoreManager.Write();
            SceneManager.LoadScene("Menu");
        }

    }
}
