using UnityEngine;

public class StageAchievementChecker : MonoBehaviour
{
    [SerializeField] private ScoreData scoreData;
    [SerializeField] private PlayerController player;
    [SerializeField] private string stageName;
    [SerializeField] private float checkInterval = 1.5f;
    [SerializeField] private StageSetting stageSetting;
    [SerializeField] private StageAchievementChecker achievementChecker;


    private bool stageCleared = false;
    private float checkTimer = 0f;
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
        if (stageSetting == null) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return; // インターバル待ち

        checkTimer = 0f; // タイマーリセット

        bool scoreOk = IsScoreAchieved();
        bool hpOk = IsHpAchieved();
        bool timeOk = IsTimeAchieved();

        // 部分達成のログ
        Debug.Log($"スコア: {scoreOk}, HP: {hpOk}, タイム: {timeOk}");

       
        stageSetting.scoreAchieved = scoreOk;
        stageSetting.hpAchieved = hpOk;
        stageSetting.clearTimeAchieved = timeOk;

        if (!stageCleared && IsAllAchieved())
        {
            stageCleared = true;
            OnStageClear();
        }
       
    }

    public void OnStageClear()
    {
        if (stageSetting == null)
        {
            Debug.LogError("StageSettingが設定されていません。");
            return;
        }

        // 判定結果をScriptableObjectに反映
        stageSetting.scoreAchieved = IsScoreAchieved();
        stageSetting.hpAchieved = IsHpAchieved();
        stageSetting.clearTimeAchieved = IsTimeAchieved();

        bool allOk = IsAllAchieved();

        Debug.Log($"スコア達成: {stageSetting.scoreAchieved}");
        Debug.Log($"HP達成: {stageSetting.hpAchieved}");
        Debug.Log($"タイム達成: {stageSetting.clearTimeAchieved}");
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
