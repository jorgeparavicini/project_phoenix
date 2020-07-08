using UnityEngine;

namespace Phoenix.Level
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        public float gameDuration = 120;
        public float gameStartDelay = 3;
        public bool autoStart = false;
    }
}