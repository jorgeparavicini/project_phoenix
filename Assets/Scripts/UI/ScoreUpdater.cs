using System.Collections;
using System.Collections.Generic;
using EventArgs;
using Score;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreUpdater : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;

    void Start()
    {
        ScoreText = GetComponent<TextMeshProUGUI>();
        ScoreManager.ScoreUpdated += OnScoreUpdated;
        ScoreText.text = "0";
    }

    private void OnScoreUpdated(object sender, ScoreUpdatedEventArgs e)
    {
        ScoreText.text = $"{e.NewScore}";
    }
}
