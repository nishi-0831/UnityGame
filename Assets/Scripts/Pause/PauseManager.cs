using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // シングルトン設定 
    // 他のクラス（例：Player）から呼び出すためのグローバル参照
    public static PauseManager Instance { get; private set; }

    [SerializeField] private GameObject pauseUI; // ポーズ画面のCanvasを指定
    private bool isPaused = false;               // 現在ポーズ中かどうか

    void Awake()
    {
        // シングルトンのインスタンスをセット
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーン遷移しても破棄されないようにする
    }

    void Update()
    {
        // 「Escキー」でポーズのON/OFF切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ClosePauseScreen();
            else
                OpenPauseScreen();
        }
    }

    /// <summary>
    /// ポーズ画面を開く（外部からも呼び出せる）
    /// </summary>
    public void OpenPauseScreen()
    {
        if (pauseUI != null)
            pauseUI.SetActive(true);

        Time.timeScale = 0f; // ゲームを止める
        isPaused = true;
    }

    /// <summary>
    /// ポーズ画面を閉じる（外部からも呼び出せる）
    /// </summary>
    public void ClosePauseScreen()
    {
        if (pauseUI != null)
            pauseUI.SetActive(false);

        Time.timeScale = 1f; // ゲームを再開
        isPaused = false;
    }

    /// <summary>
    /// 「Resume」ボタン：ゲームを再開
    /// </summary>
    public void ResumeGame()
    {
        ClosePauseScreen();
    }

    /// <summary>
    /// 「MainMenu」ボタン：メインメニューへ戻る
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // シーン名はプロジェクトに合わせて変更
    }

    /// <summary>
    /// 「StageSelect」ボタン：ステージ選択画面へ
    /// </summary>
    public void GoToStageSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelect"); // シーン名はプロジェクトに合わせて変更
    }
}
