using UnityEngine;
using UnityEngine.UI; // トグルとイメージを使うために必要

public class CheckToStar : MonoBehaviour
{
    // インスペクターから設定するための公開変数
    [Header("関連するUI要素")]
    public Toggle checkToggle;     // Check1のトグル
    public Image starImage;       // Star1のImageコンポーネント

    [Header("画像スプライト")]
    public Sprite offSprite;      // 黒い星の画像
    public Sprite onSprite;       // 黄色い星の画像

    // 初期化時にトグルのイベントを登録する
    void Start()
    {
        // 変数にUI要素が設定されているか確認
        if (checkToggle != null)
        {
            // トグルの値が変化したときに、OnToggleValueChangedメソッドを呼び出すように登録
            checkToggle.onValueChanged.AddListener(OnToggleValueChanged);

            // 起動時の初期状態を反映
            UpdateStarSprite(checkToggle.isOn);
        }
        else
        {
            Debug.LogError("Toggleが設定されていません！インスペクターを確認してください。");
        }
    }

    // トグルの値が変化したときに呼ばれるメソッド
    void OnToggleValueChanged(bool isOn)
    {
        UpdateStarSprite(isOn);
    }

    // 星の画像を更新する処理
    void UpdateStarSprite(bool isOn)
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