using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUi : MonoBehaviour
{
    public GameObject MenuObject;
    public GameObject ScoreObject;

    private void Start()
    {
        MenuObject.SetActive(true);
        ScoreObject.SetActive(false);
    }

    public void StartLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void ShowScores()
    {
        MenuObject.SetActive(false);
        ScoreObject.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
