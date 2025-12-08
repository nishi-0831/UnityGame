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
        toggle.isOn = stageAchievementChecker.IsTimeAchieved();
    }
    public void ScoreOnToggle()
    {
        toggle.isOn = stageAchievementChecker.IsScoreAchieved();
    }
    public void HpOnToggle()
    {
        toggle.isOn = stageAchievementChecker.IsHpAchieved();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
