using System;
using UnityEngine;

public class GameOutcomeManager : MonoBehaviour
{
    public static GameOutcomeManager Instance { get; private set; }

    private event Action OnGameClear;
    private event Action OnGameOver;
    [SerializeField] private float transitionSceneDelay;
    

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
        TransitionScene.Instance.ToResult(transitionSceneDelay);
    }

    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
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
