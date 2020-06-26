using Level;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GameOverMessage : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        
        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.enabled = false;
        }

        private void OnEnable()
        {
            LevelManager.GameOver += LevelManagerOnGameOver;
        }

        private void OnDisable()
        {
            LevelManager.GameOver -= LevelManagerOnGameOver;
        }

        private void LevelManagerOnGameOver(object sender, System.EventArgs e)
        {
            _text.enabled = true;
        }
    }
}
