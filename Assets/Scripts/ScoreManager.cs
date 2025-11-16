using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScoreManager;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    [SerializeField] private ScoreData scoreData_;
    
    private bool isStartedCountClearTime = false;
    [SerializeField] private float startTime_;
    [SerializeField] private float remainingTime_;
    [SerializeField] private float endTime_;
    [SerializeField] private TextMeshProUGUI timerText_;
    private Action timeUp_;
    private bool countTime_;
    //クリア時間の計測を始める
    //[SerializeField] private 

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else 
        {
            return;
        }
        if (scoreData_ == null)
        {
            Debug.LogError("ScoreData is null!!!");
            return;
        }
        scoreData_.Initialize();
        StartCountClearTime();
        if(StageUIManager.Instance)
        {
            remainingTime_ = StageUIManager.Instance.CurrentStageSetting().timeLimit;
        }
    }
    public void RegisterOnTimeUpCallback(Action callback)
    {
        timeUp_ = callback;
    }
    public void StartCountClearTime()
    {
        if(isStartedCountClearTime)
        {
            return;
        }
        
        startTime_ = Time.time;
        isStartedCountClearTime = true;
    }
    public void EndCountClearTime()
    {
        endTime_ = Time.time;
        isStartedCountClearTime = false;
        
        scoreData_.clearTime = endTime_ - startTime_;
        Debug.Log($"ClearTime:{scoreData_.clearTime}!!!");
    }
    // Update is called once per frame
    void Update()
    {
        if(isStartedCountClearTime == false)
            return;
        
        scoreData_.clearTime = Time.time - startTime_;
        if(remainingTime_ > 0) 
        {
            CountDownTimer();
        }
    }

    public void CountDownTimer()
    {
        remainingTime_ -= Time.deltaTime;
        if (remainingTime_ > 0)
        {
            //カウントダウンタイマーの時間表示
            timerText_.text = $"Time:{Math.Truncate(remainingTime_)}";
        }
        else if (remainingTime_ <= 0)
        {
            //カウントダウンタイマーが０になった時の処理
            timeUp_?.Invoke();
        }
    }

    public void ReceiveScore(int value)
    {
        scoreData_.score += value;
    }
}
