using System;
using System.Collections;
using EventArgs;
using Level;
using Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VR;

namespace UI
{
    public class GameStatusUi : MonoBehaviour
    {
        public GameObject GameOverContent;
        private LaserPointerVrButtonHandler HandLaserPointer;

        private void OnEnable()
        {
            LevelManager.GameOver += OnGameOver;
            LevelManager.GameStarting += OnGameStarting;
        }

        private void OnDisable()
        {
            LevelManager.GameOver -= OnGameOver;
            LevelManager.GameStarting -= OnGameStarting;
        }

        private void Awake()
        {
            HandLaserPointer = GameObject.FindWithTag("Pointer").GetComponent<LaserPointerVrButtonHandler>();
        }


        private void OnGameStarting(object sender, System.EventArgs e)
        {
            HandLaserPointer.DisablePointer();
        }

        private void OnGameOver(object sender, System.EventArgs e)
        {
            GameOverContent.SetActive(true);
        }

        public void StartGame_Button()
        {
            LevelManager.StartGame();
        }
    }
}
