using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RetryStage()
    {
        if (GameSessionManager.Instance != null)
        {
            string stageSceneName = GameSessionManager.Instance.GetCurrentStageSceneName();
            if (!string.IsNullOrEmpty(stageSceneName))
            {
                SceneManager.LoadScene(stageSceneName);
                return;
            }
        }

        // ステージの情報取得に失敗した場合:ステージ選択シーンに戻す
        Debug.LogWarning("Stage scene name not found, returning to stage select");
        TransitionScene.Instance?.ToStageSelect();
    }
}