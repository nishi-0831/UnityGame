using UnityEngine;
using UnityEngine.UI;

public class ToggleConroller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private StageAchievementChecker stageAchievementChecker;
    [SerializeField] private Toggle toggle;
    void Start()
    {
        
    }

    public void TimeOnToggle()
    {
        if(stageAchievementChecker.IsTimeAchieved())
        {
            toggle.isOn = true;
        }
    }
    public void ScoreOnToggle()
    {
        if (stageAchievementChecker.IsScoreAchieved())
        {
            toggle.isOn = true;
        }
    }
    public void HpOnToggle()
    {
        if (stageAchievementChecker.IsHpAchieved())
        {
            toggle.isOn = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
