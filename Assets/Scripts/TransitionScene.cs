using UnityEngine;
using UnityEngine.SceneManagement;
public class TransitionScene : MonoBehaviour
{
    [System.Serializable]
    public struct SceneName
    {
        public const string title = "Title";
        public const string play = "SampleScene";
        public const string gameOver = "GameOver";
        public const string result = "Result";
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransitionTitle()
    {
        SceneManager.LoadScene(SceneName.title);
    }
    public void TransitionPlay()
    {
        SceneManager.LoadScene(SceneName.play);
    }
    public void TransitionGameOver()
    {
        SceneManager.LoadScene(SceneName.gameOver);
    }
    public void TransitionResult()
    {
        SceneManager.LoadScene(SceneName.result);
    }
}
