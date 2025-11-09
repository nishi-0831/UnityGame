using System;
using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance { get; private set; }

    [System.Serializable]
    public struct SceneName
    {
        public const string title = "Title";
        public const string play = "State1";
        public const string gameOver = "GameOver";
        public const string result = "Result";
        public const string stageSelect = "StageSelect";
        public const string pause = "Pause";
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("QuitGame");
            QuitGame();
        }
    }

    public void ToTitle(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay, () => SceneManager.LoadScene(SceneName.title)));
    }
    public void ToPlay(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay, () => SceneManager.LoadScene(SceneName.play)));
    }
    public void ToGameOver(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay, () => SceneManager.LoadScene(SceneName.gameOver)));
    }
    public void ToResult(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay,() => SceneManager.LoadScene(SceneName.result)));
    }
    public void ToStageSelect(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay, () => SceneManager.LoadScene(SceneName.stageSelect)));
    }
    public void ToPause(float delay = 0.0f)
    {
        StartCoroutine(ExecuteAfterDelay(delay, () => SceneManager.LoadScene(SceneName.pause)));
    }
    IEnumerator ExecuteAfterDelay(float delay,Action cb)
    {
        yield return new WaitForSeconds(delay);
        cb();
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();

#endif
    }
}
