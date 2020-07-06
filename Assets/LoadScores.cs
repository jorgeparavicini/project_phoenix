using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Score;
using UnityEngine;
using Valve.VR.InteractionSystem;

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
            Destroy(child.gameObject);
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
            userScores.ForEach((userScore =>
            {
                script.WriteScoresScrollable(userScore.UserName, userScore.Score);
            }));
            
            prefab.transform.SetParent(transform, false);
        }));
    }
}
