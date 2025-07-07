using System.Collections;
using UnityEngine;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    [SerializeField] private ScoreData scoreData_;
    

    private bool isStartedCountClearTime = false;
    [SerializeField] private float startTime_;
    [SerializeField] private float endTime_;
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
        //デバッグ
        if(Input.GetKeyDown(KeyCode.T))
        {
            StartCountClearTime();
        }
        else if(Input.GetKeyDown(KeyCode.Y))
        {
            EndCountClearTime();
        }
        scoreData_.clearTime = Time.time - startTime_;
    }
    
    public void ReceiveScore(int value)
    {
        scoreData_.score += value;
    }
}
