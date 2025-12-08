using UnityEngine;

public class StageAchievementChecker : MonoBehaviour
{
    [SerializeField] private ScoreData scoreData;
    [SerializeField] private PlayerController player;

    [SerializeField] private StageSetting stageSetting;


    void Awake()
    {
        scoreData = Resources.Load<ScoreData>("ScoreData");
        if(scoreData == null )
        {
            Debug.LogError("ResourcesにScoreDataが見当たりません");
        }
        //stageSetting = Resources.Load<StageSetting>($"StageSettings/{stageName}");
        //if (stageSetting == null)
        //{
        //    Debug.LogError($"StageSettings/{stageName}.asset が見つかりません。");
        //}
    }
    private void Start()
    {
        GameOutcomeManager.Instance?.RegisterGameClearCallback(OnStageClear);
        stageSetting = StageUIManager.Instance?.CurrentStageSetting();
        //stageSetting.InitAchievement();
    }

    public bool IsScoreAchieved()
    {
        if (stageSetting == null) return false;
        return scoreData.score >= stageSetting.scoreTarget;
    }

    public bool IsHpAchieved() // HP達成
    {
        if (stageSetting == null) return false;
        return player.Hp >= stageSetting.hpTarget;
    }

    public bool IsTimeAchieved() // タイム達成
    {
        if (stageSetting == null) return false;
        return scoreData.clearTime <= stageSetting.clearTimeLimit;
    }

    public bool IsAllAchieved() // 全て達成しているか
    {
        return IsScoreAchieved() && IsHpAchieved() && IsTimeAchieved();
    }

    void Update()
    {
       
       
    }

    public void OnStageClear()
    {
        if (stageSetting == null)
        {
            Debug.LogError("StageSettingが設定されていません。");
            return;
        }

        // 判定結果をScriptableObjectに反映
        bool scoreOk = IsScoreAchieved();
        bool timeOk = IsTimeAchieved();
        bool hpOk = IsHpAchieved();
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
            //ランク表示・報酬処理などここで呼び出し
    }

        //ScriptableObjectの内容を保存（エディタ実行中のみ）
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(stageSetting);
#endif
    }
}
