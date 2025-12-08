using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームセッション中のステージ情報を管理
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    private string currentStageSceneName;
    private int currentStageIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 現在のステージ情報を設定
    /// </summary>
    public void SetCurrentStage(string stageSceneName, int stageIndex)
    {
        currentStageSceneName = stageSceneName;
        currentStageIndex = stageIndex;
        Debug.Log($"GameSession: Stage set to {stageSceneName} (Index: {stageIndex})");
    }

    /// <summary>
    /// 現在のステージシーン名を取得
    /// </summary>
    public string GetCurrentStageSceneName() => currentStageSceneName;

    /// <summary>
    /// 現在のステージインデックスを取得
    /// </summary>
    public int GetCurrentStageIndex() => currentStageIndex;

    /// <summary>
    /// ステージの情報をリセット
    /// </summary>
    public void ResetSession()
    {
        currentStageSceneName = null;
        currentStageIndex = -1;
    }
}