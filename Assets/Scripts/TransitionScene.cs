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
        public const string play = "SampleScene";
        public const string gameOver = "GameOver";
        public const string result = "Result";
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
        
    }

    public void ToTitle()
    {
        SceneManager.LoadScene(SceneName.title);
    }
    public void ToPlay()
    {
        SceneManager.LoadScene(SceneName.play);
    }
    public void ToGameOver()
    {
        SceneManager.LoadScene(SceneName.gameOver);
    }
    public void ToResult()
    {
        SceneManager.LoadScene(SceneName.result);
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
