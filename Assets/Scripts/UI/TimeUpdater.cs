using Phoenix.EventArgs;
using Phoenix.Level;
using TMPro;
using UnityEngine;

namespace Phoenix.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TimeUpdater : MonoBehaviour
    {
        private TextMeshProUGUI TimeText;
        void Start()
        {
            TimeText = GetComponent<TextMeshProUGUI>();
            LevelManager.TimeUpdated += OnTimeUpdated;
            TimeText.text = $"{LevelManager.GameDuration:F1}";
        }
    
        private void OnTimeUpdated(object sender, TimeUpdatedEventArgs e)
        {
            TimeText.text = $"{e.TotalTime - e.Time:F1}";
        }
    }
}
