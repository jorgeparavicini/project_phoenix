using System;
using EventArgs;
using Level;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TimeUpdater : MonoBehaviour
    {
        private TextMeshProUGUI _timeText;
        
        private void Awake()
        {
            _timeText = GetComponent<TextMeshProUGUI>();

            _timeText.enabled = true;
            _timeText.text = $"{LevelManager.GameDuration:F1}";
        }

        private void OnEnable()
        {
            LevelManager.TimeUpdated += OnTimeUpdated;
            LevelManager.GameOver += LevelManagerOnGameOver;
        }

        private void OnDisable()
        {
            LevelManager.TimeUpdated -= OnTimeUpdated;
            LevelManager.GameOver -= LevelManagerOnGameOver;
        }

        private void OnTimeUpdated(object sender, TimeUpdatedEventArgs e)
        {
            _timeText.text = $"{e.TotalTime - e.Time:F1}";
        }
        
        private void LevelManagerOnGameOver(object sender, System.EventArgs e)
        {
            _timeText.enabled = false;
        }
    }
}
