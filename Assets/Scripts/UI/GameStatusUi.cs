using Phoenix.Level;
using Phoenix.VR;
using UnityEngine;

namespace Phoenix.UI
{
    public class GameStatusUi : MonoBehaviour
    {
        public GameObject GameStartContent;
        public GameObject GameStartButton;
        private LaserPointerVrButtonHandler HandLaserPointer;

        private void OnEnable()
        {
            LevelManager.GameStarting += OnGameStarting;
            LevelManager.GameStarted += OnGameStarted;
        }

        private void OnDisable()
        {
            LevelManager.GameStarting -= OnGameStarting;
            LevelManager.GameStarted -= OnGameStarted;
        }

        private void Awake()
        {
            HandLaserPointer = GameObject.FindWithTag("Pointer").GetComponent<LaserPointerVrButtonHandler>();
        }
        

        private void OnGameStarted(object sender, System.EventArgs e)
        {
            GameStartContent.SetActive(false);
        }

        private void OnGameStarting(object sender, System.EventArgs e)
        {
            HandLaserPointer.DisablePointer();
            GameStartButton.SetActive(false);
        }

        public void StartGame_Button()
        {
            LevelManager.StartGame();
        }
    }
}
