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
            Instance.ClosePauseScreen();
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーン遷移しても破棄されないようにする
        Instance.ClosePauseScreen();
    }


    void Update()
    {
        // 「Escキー」でポーズのON/OFF切り替え
        if (Input.GetKeyDown(KeyCode.Tab))
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
        Debug.Log("Close");
        if (pauseUI != null)
            pauseUI.SetActive(false);

        Time.timeScale = 1f; // ゲームを再開
        isPaused = false;
    } 
}
