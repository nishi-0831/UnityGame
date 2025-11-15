using UnityEngine;

public class StageAchievementChecker : MonoBehaviour
{
    [SerializeField] private ScoreData scoreData;
    [SerializeField] private PlayerController player;
    [SerializeField] private string stageName;

    [SerializeField] private StageSetting stageSetting;
    [SerializeField] private StageAchievementChecker achievementChecker;


    void Awake()
    {
        Debug.Log("ABC");
       
        stageSetting = Resources.Load<StageSetting>($"StageSettings/{stageName}");
        if (stageSetting == null)
        {
            Debug.LogError($"StageSettings/{stageName}.asset が見つかりません。");
        }
    }

    void StageClear()
    {
        // スコア集計などを終えたあと
        achievementChecker.OnStageClear(); // ←ここで上の関数が実行される！
    }

    public bool IsScoreAchieved()
    {
        if (stageSetting == null) return false;
        return scoreData.score >= stageSetting.scoreTarget;
    }

    public bool IsHpAchieved()
    {
        if (stageSetting == null) return false;
        return player.Hp() >= stageSetting.hpTarget;
    }

    public bool IsTimeAchieved()
    {
        if (stageSetting == null) return false;
        return scoreData.clearTime <= stageSetting.clearTimeLimit;
    }

    public bool IsAllAchieved()
    {
        return IsScoreAchieved() && IsHpAchieved() && IsTimeAchieved();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnStageClear();
        }
       
    }
    public void OnStageClear()
    {
        bool scoreOk = IsScoreAchieved();
        bool hpOk = IsHpAchieved();
        bool timeOk = IsTimeAchieved();
        bool allOk = IsAllAchieved();

        stageSetting.achievedScoreTarget = scoreOk;
        stageSetting.achievedClearTimeLimit = timeOk;
        stageSetting.achievedHpTarget = hpOk;
        Debug.Log($"スコア達成: {scoreOk}");
        Debug.Log($"HP達成: {hpOk}");
        Debug.Log($"タイム達成: {timeOk}");
        Debug.Log($"全条件達成: {allOk}");

        if (allOk)
        {
            Debug.Log("★すべての条件を達成しました！★");
            // ここで報酬付与やランク表示など
        }
    }

}
