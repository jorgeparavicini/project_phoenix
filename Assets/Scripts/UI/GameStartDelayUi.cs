using Phoenix.EventArgs;
using Phoenix.Level;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class GameStartDelayUi : MonoBehaviour
    {
        private TextMeshProUGUI _text;
    
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.enabled = false;
        }

        private void OnEnable()
        {
            LevelManager.GameStarting += LevelManagerOnGameStarting;
            LevelManager.GameStarted += LevelManagerOnGameStarted;
            LevelManager.GameStarterTimeUpdated += LevelManagerOnGameStarterTimeUpdated;
        }

        private void OnDisable()
        {
            LevelManager.GameStarting -= LevelManagerOnGameStarting;
            LevelManager.GameStarted -= LevelManagerOnGameStarted;
        }

        private void LevelManagerOnGameStarting(object sender, System.EventArgs e)
        {
            _text.enabled = true;
        }

        private void LevelManagerOnGameStarted(object sender, System.EventArgs e)
        {
            _text.enabled = false;
        }
    
        private void LevelManagerOnGameStarterTimeUpdated(object sender, TimeUpdatedEventArgs e)
        {
            _text.text = $"{e.Time:0}";
        }
    }
}
