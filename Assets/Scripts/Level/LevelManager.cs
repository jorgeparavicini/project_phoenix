using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Phoenix.EventArgs;
using Phoenix.Level.Packages;
using Phoenix.Score;
using UnityEngine;
using Valve.VR.InteractionSystem;

[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace Phoenix.Level
{
    [RequireComponent(typeof(ScoreManager))]
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        private float _currentGameTime;
        private bool _isGameOver = false;
        private bool _hasGameStarted = false;

        // Required by the High Score Manager to initialize the score json file.
        [SerializeField] internal string _levelName;
        [SerializeField] private float _gameDuration = 120;
        [SerializeField] private float _gameStartDelay = 3;
        [SerializeField] private bool _autoStart = false;


        public static bool IsGameOver => _instance._isGameOver;
        public static bool HasGameStarted => _instance._hasGameStarted;


        public static float GameDuration => _instance._gameDuration;
        public static float GameStartDelay => _instance._gameStartDelay;
        public static string LevelName => _instance._levelName;


        public static event EventHandler<TimeUpdatedEventArgs> TimeUpdated = delegate { };
        public static event EventHandler GameOver = delegate { };
        public static event EventHandler<TimeUpdatedEventArgs> GameStarterTimeUpdated = delegate { };
        public static event EventHandler GameStarting = delegate { };
        public static event EventHandler GameStarted = delegate { };


        private void Awake()
        {
            // TODO: Check with scene manager when new scene loaded.
            if (_instance != null) Debug.LogWarning("Overriding");
            _instance = this;

            GameOver += GameOverCleanUp;
        }

        private void Start()
        {
            if (_autoStart) StartGame();
        }


        public static void StartGame()
        {
            _instance.StartCoroutine(StartGameInternal());
        }

        private static IEnumerator StartGameInternal()
        {
            if (HasGameStarted) yield break;
            GameStarting(_instance, System.EventArgs.Empty);

            var delay = GameStartDelay;
            while (delay > 0)
            {
                GameStarterTimeUpdated(_instance, new TimeUpdatedEventArgs(delay, GameStartDelay));
                delay -= 1f;
                yield return new WaitForSeconds(1f);
            }

            GameObject.FindGameObjectsWithTag(PackageSpawner.Tag)
                      .ForEach(spawner => spawner.GetComponent<PackageSpawner>().StartSpawner());

            _instance._hasGameStarted = true;
            GameStarted(_instance, System.EventArgs.Empty);
        }


        private void Update()
        {
            if (!HasGameStarted) return;
            if (IsGameOver) return;

            _currentGameTime += Time.deltaTime;
            TimeUpdated(this, new TimeUpdatedEventArgs(_currentGameTime, _gameDuration));

            if (!(_currentGameTime >= _gameDuration)) return;

            _isGameOver = true;
            GameOver(this, System.EventArgs.Empty);
        }


        protected virtual void GameOverCleanUp(object sender, System.EventArgs e)
        {
            GameObject.FindGameObjectsWithTag(Package.Tag)
                      .ForEach(package => package.GetComponent<Package>().Destroy());
            GameObject.FindGameObjectsWithTag(PackageSpawner.Tag)
                      .ForEach(spawner => spawner.GetComponent<PackageSpawner>().Stop());
        }
    }
}
