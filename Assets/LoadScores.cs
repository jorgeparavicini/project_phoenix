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
        var scores = HighScoreManager.LevelScores;
        scores.ForEach((score =>
        {
            Instantiate(ScoresPrefab).transform.SetParent(transform);
        }));
    }
}
