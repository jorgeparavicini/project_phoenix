using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    public Animator Transition;
    public float TransitionTime;
    private static LevelLoader _instance;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
            Debug.LogError("Multiple Level Loaders found. This is not supported");
        }
    }

    public static void LoadLevel(string levelName)
    {
        _instance.StartCoroutine(LoadLevelInternal(levelName));
    }

    private static IEnumerator LoadLevelInternal(string levelName)
    {
        _instance.Transition.SetTrigger(Start);
        yield return new WaitForSeconds(_instance.TransitionTime);

        SceneManager.LoadScene(levelName);
    }
}
