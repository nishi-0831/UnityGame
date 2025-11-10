using System;
using UnityEngine;

public class GameOutcomeManager : MonoBehaviour
{
    public static GameOutcomeManager Instance { get; private set; }

    private event Action OnGameClear;
    private event Action OnGameOver;
    [SerializeField] private float transitionSceneDelay;
    [SerializeField] private AudioClip gameOverSE;
    [SerializeField][Range(0.0f,1.0f)] private float gameOverSEVolume = 0.5f;
    [SerializeField] private AudioClip gameClearSE;
    [SerializeField][Range(0.0f, 1.0f)] private float gameClearSEVolume = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance != null  && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(this.gameObject);
    }

    public void RegisterGameClearCallback(Action cb) => OnGameClear += cb;
    public void RegisterGameOverCallback(Action cb) => OnGameOver += cb;

    public void TriggerGameClear()
    {
        OnGameClear?.Invoke();
        AudioManager.Instance.PlaySound(gameClearSE, gameClearSEVolume);
        TransitionScene.Instance.ToResult(transitionSceneDelay);
    }

    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
        AudioManager.Instance.PlaySound(gameOverSE, gameOverSEVolume);
        TransitionScene.Instance.ToPause(transitionSceneDelay);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
