using System.Linq;
using Level;
using Score;
using UnityEditor;
using UnityEngine;

namespace EditorScripts.Editor
{
    public class ScoreManagerWindow : EditorWindow
    {
        private const string LevelPrefabsLocation = "Prefabs/Levels";

        [MenuItem("SwissSkillsVR/Score Manager")]
        private static void Init()
        {
            var window = GetWindow<ScoreManagerWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Write")) HighScoreManager.Write();

            if (GUILayout.Button("Read"))
            {
                HighScoreManager.ReadScores();
                Debug.Log(HighScoreManager.LevelScores.First());
            }

            if (GUILayout.Button("Initialize Save Data"))
            {
                var levelPrefabs = Resources.LoadAll<GameObject>(LevelPrefabsLocation);
                var levelManagers = levelPrefabs.Select(level => level.GetComponentInChildren<LevelManager>()).ToList();
                levelManagers.ForEach(levelManager => HighScoreManager.InitializeLevel(levelManager._levelName, false));
                HighScoreManager.Write();
            }
        }
    }
}
