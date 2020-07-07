using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Phoenix.Level;
using Phoenix.Score;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Phoenix.UI
{
    public class HighScoreListUi : MonoBehaviour
    {
        private readonly List<UserScoreUi> _scoreObjects = new List<UserScoreUi>();

        public Transform Content;
        public GameObject UserScoreUiPrefab;

        public ReadOnlyCollection<UserScoreUi> GetScoreObjects => _scoreObjects.AsReadOnly();

        public void AddScore(UserScore score)
        {
            var instance = Instantiate(UserScoreUiPrefab, Content);
            var ui = instance.GetComponent<UserScoreUi>();
            ui.Score = score;
            _scoreObjects.Add(ui);
        }

        public void ClearScores()
        {
            _scoreObjects.Clear();
            foreach (Transform child in Content) Destroy(child);
        }

        public void SetScoresForLevel(string levelName)
        {
            ClearScores();
            var levelScore = HighScoreManager.LevelScores.Where(x => x.LevelName == levelName).ToList();
            if (!levelScore.Any())
            {
                Debug.LogError($"No stored scores found for level: {levelName}");
                return;
            }

            if (levelScore.Count > 1)
            {
                Debug.LogError($"Multiple score entries found for {levelName}");
                return;
            }

            levelScore.First().Scores.ForEach(AddScore);
        }

        public void SetScoresForCurrentLevel()
        {
            // Get the latest scores from the stored json file.
            SetScoresForLevel(LevelManager.LevelName);
        }
    }
}
