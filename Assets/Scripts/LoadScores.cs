using System.Collections.Generic;
using Phoenix.Score;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Phoenix
{
    public class LoadScores : MonoBehaviour
    {

        public GameObject ScoresPrefab;

        private void OnEnable()
        {
            RemoveChildren();
        
            LoadScorePrefabs();
        }

        private void RemoveChildren()
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("ScorePrefab"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void LoadScorePrefabs()
        {
            var levelScores = HighScoreManager.LevelScores;
            levelScores.ForEach((levelScore =>
            {
                var prefab = Instantiate(ScoresPrefab);
                var script = prefab.GetComponent<WriteScores>();
                script.WriteTitle(levelScore.LevelName);

                var userScores = levelScore.Scores;
                List<string> userNames = new List<string>();
                List<int> userScoresInt = new List<int>();
                userScores.ForEach((userScore =>
                {
                    userNames.Add(userScore.UserName);
                    userScoresInt.Add(userScore.Score);
                }));

                script.WriteScoresScrollable(userNames, userScoresInt);
            
                prefab.transform.SetParent(transform, false);
                prefab.transform.SetSiblingIndex(0);
            }));
        }
    }
}
