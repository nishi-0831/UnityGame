using UnityEngine;
using UnityEngine.UI;

public class ScoreGetter : MonoBehaviour
{
    [Header("各星に対応するトグル")]
    [SerializeField] private Toggle check1;
    [SerializeField] private Toggle check2;
    [SerializeField] private Toggle check3;

    [Header("対象ステージ番号")]
    [SerializeField] private int stageNumber = 1;

    void Start()
    {
        // データ読み取り
        int score = PlayerPrefs.GetInt($"Stage{stageNumber}_Score", -1);
        float time = PlayerPrefs.GetFloat($"Stage{stageNumber}_Time", 9999f);
        int life = PlayerPrefs.GetInt($"Stage{stageNumber}_Life", -1);

        Debug.Log($"[Stage{stageNumber}] 読み込み: Score={score}, Time={time}, Life={life}");

        // 条件チェック
        check1.isOn = (time <= 120f);
        check2.isOn = (score >= 1000);
        check3.isOn = (life >= 2);
    }
}