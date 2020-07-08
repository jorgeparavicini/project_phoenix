using System;
using System.Collections;
using System.Collections.Generic;
using Phoenix.Level;
using UnityEngine;

public class GameStartUi : MonoBehaviour
{
    public void Start()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        LevelManager.GameStarted += LevelManagerOnGameStarted;
    }

    private void OnDisable()
    {
        LevelManager.GameStarted -= LevelManagerOnGameStarted;
    }

    private void LevelManagerOnGameStarted(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
    }
}
