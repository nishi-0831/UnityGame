using UnityEngine;
using TMPro;

public class GameResultManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private StageSetting stageSetting;  // ScriptableObject参照
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI gameClearText;

    [Header("現在の状態")]
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentHP = 0;

    private bool isGameOver = false;
    private bool isGameClear = false;

    void Start()
    {
        gameOverText.gameObject.SetActive(false);
        gameClearText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGameOver || isGameClear) return;

        elapsedTime += Time.deltaTime;

        // --- Game Over 判定 ---
        if (currentHP <= 0 && stageSetting.scoreTarget > 0)
        {
            ShowGameOver("HPが0になりました！");
            return; // Game Over が出たら処理終了
        }

        if (stageSetting.clearTimeLimit > 0 && elapsedTime >= stageSetting.clearTimeLimit)
        {
            ShowGameOver("時間切れ！");
            return; // 時間切れで Game Over
        }

        // --- Game Clear 判定 ---
        bool scoreOk = currentScore > stageSetting.scoreTarget; // スコアが目標以上
        bool hpOk = currentHP > 0;                               // HPが残っている
        bool timeOk = stageSetting.clearTimeLimit <= 0 || elapsedTime <= stageSetting.clearTimeLimit; // 時間制限なしor間に合っている

        if (scoreOk && hpOk && timeOk)
        {
            ShowGameClear();
        }
    }

    // --- ステータス更新用メソッド ---
    public void AddScore(int score)
    {
        currentScore += score;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }

    // --- 結果表示 ---
    private void ShowGameOver(string reason = "")
    {
        isGameOver = true;
        //gameOverText.text = $"Game Over\n{reason}";
        gameOverText.gameObject.SetActive(true);
    }

    private void ShowGameClear()
    {
        isGameClear = true;
        gameClearText.gameObject.SetActive(true);
    }
}
