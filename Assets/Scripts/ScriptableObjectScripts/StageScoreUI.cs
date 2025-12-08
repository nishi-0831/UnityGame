using UnityEngine;
using UnityEngine.UI; // トグルとイメージを使うために必要
using TMPro;
using System.Collections.Generic;

public class StageScoreUI : MonoBehaviour
{
    //public StageSetting stageSetting;
    [Header("画像スプライト")]
    public Sprite offSprite;      // 黒い星の画像
    public Sprite onSprite;       // 黄色い星の画像
    [System.Serializable]
    public struct ScoreUI
    {
        [Header("関連するUI要素")]
        public Toggle checkToggle;     // トグル
        public Image starImage;       // 
        public Text text; // Check1

        [Header("画像スプライト")]
        public Sprite offSprite;      // 黒い星の画像
        public Sprite onSprite;       // 黄色い星の画像
        /// <summary>
        /// 星の画像を更新する処理
        /// </summary>
        /// <param name="isOn"></param>
        public void UpdateStarSprite(bool isOn)
        {
            if (starImage == null)
            {
                Debug.LogError("Star Imageが設定されていません！インスペクターを確認してください。");
                return;
            }

            // isOnがtrue（ON）なら黄色い星、false（OFF）なら黒い星に切り替え
            if (isOn)
            {
                starImage.sprite = onSprite;
            }
            else
            {
                starImage.sprite = offSprite;
            }
        }
    }
    public ScoreUI[] scoreUIs = new ScoreUI[3];

    /// <summary>
    /// ステージの達成状況を全てのUIに反映させる
    /// </summary>
    /// <param name="stageSetting"></param>
    public void ApplyStageSetting(StageSetting stageSetting)
    {
        Debug.Log("ApplyStageSetting");
        ApplyAllStar(stageSetting);
        ApplyAllText(stageSetting);
    }
    /// <summary>
    /// ステージの達成状況を星の画像に反映させる
    /// </summary>
    /// <param name="stageSetting"></param>
    public void ApplyAllStar(StageSetting stageSetting)
    {
        for (int i = 0; i < scoreUIs.Length; i++)
        {
            scoreUIs[i].offSprite = offSprite;
            scoreUIs[i].onSprite = onSprite;
        }
        CheckStar(scoreUIs[0], stageSetting.achievedScoreTarget);
        CheckStar(scoreUIs[1], stageSetting.achievedClearTimeLimit);
        CheckStar(scoreUIs[2], stageSetting.achievedHpTarget);
    }
    /// <summary>
    /// ステージの実績を達成する状況をテキストに反映させる
    /// </summary>
    /// <param name="stageSetting"></param>
    public void ApplyAllText(StageSetting stageSetting)
    {
        scoreUIs[0].text.text = $"スコア{stageSetting.scoreTarget}以上でクリア";
        scoreUIs[1].text.text = $"{stageSetting.clearTimeLimit}秒以内でクリア";
        scoreUIs[2].text.text = $"残りライフ{stageSetting.hpTarget}以上でクリア";
    }
    void CheckStar(ScoreUI scoreUI,bool achieved)
    {
        scoreUI.UpdateStarSprite(achieved);
        scoreUI.checkToggle.isOn = achieved;
    }
}