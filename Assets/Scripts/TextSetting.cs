using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextSetting : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI_;
    [SerializeField] private GameObject gameClearUI_;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image blackPanel;

    private void Start()
    {
        // ゲームオーバー
        playerController.RegisterGameOverCallBack(StartFadeOut);
        playerController.RegisterGameOverCallBack(StartGameOverUI);

        // ゲームクリア
        playerController.RegisterGameClearCallBack(StartFadeOut);
        playerController.RegisterGameClearCallBack(StartGameClearUI);
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private void StartGameOverUI()
    {
        blackPanel.transform.SetAsFirstSibling();
        StartCoroutine(ShowUIAfterDelay(gameOverUI_));
    }

    private void StartGameClearUI()
    {
        StartCoroutine(ShowUIAfterDelay(gameClearUI_));
    }

    private IEnumerator FadeOutCoroutine()
    {
        Input.ResetInputAxes();
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        float t = 0f;
        float duration = 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; //これ重要：停止中でも動く(停止するとフェードも終わるから)
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            blackPanel.color = c;
            yield return null;
        }
       

        // ここで全体停止！
        Time.timeScale = 0f;



        // ここでGameOver UI表示
        gameOverUI_.SetActive(true);

        // UI表示と同時にSE再生
        AudioManager.Instance.PlayGameOver(0.3f);
    }


    private IEnumerator ShowUIAfterDelay(GameObject ui)
    {
        yield return new WaitForSeconds(1f);
        if (ui != null)
        {
            ui.SetActive(true);
        }
    }
}
