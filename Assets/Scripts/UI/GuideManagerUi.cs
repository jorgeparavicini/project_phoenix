using Phoenix.Level;
using UnityEngine;

namespace Phoenix.UI
{
    public class GuideManagerUi : MonoBehaviour
    {

        public GameObject GameGuide;
        public GameOverUi GameOverContainer;
        public GameObject SaveScoreContainer;

        private void Start()
        {
            LevelManager.GameOver += OnGameOver;
            LevelManager.GameOver += GameOverContainer.OnGameOver;

            GameGuide.SetActive(true);
            SaveScoreContainer.SetActive(false);
            GameOverContainer.gameObject.SetActive(false);
        }

        private void OnGameOver(object sender, System.EventArgs e)
        {
            GameGuide.SetActive(false);
            GameOverContainer.gameObject.SetActive(true);
        }

        public void ContinueAfterGameOver()
        {
            if (!LevelManager.IsGameOver)
            {
                Debug.LogError("Game is not over yet");
                return;
            }

            GameOverContainer.gameObject.SetActive(false);
            SaveScoreContainer.SetActive(true);
        }

    }
}
