using UnityEngine;

[CreateAssetMenu(fileName = "StageSetting", menuName = "Scriptable Objects/StageSetting")]
public class StageSetting : ScriptableObject
{
    [Header("クリア評価用：制限時間（秒単位）")]
    public float clearTimeLimit;

    [Header("クリア評価用：目標スコア")]
    public int scoreTarget;

    [Header("クリア評価用：目標体力（残りHP）")]
    public int hpTarget;

    public bool clearTimeAchieved;
    public bool scoreAchieved;
    public bool hpAchieved;

    


    //ここにboolで達成しているカを書く
}
