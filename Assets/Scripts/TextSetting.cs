using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextSetting : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI_;
    [SerializeField] private GameObject gameClearUI_;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image blackPanel;
    [SerializeField] private float animDuration;
    private void Start()
    {
        // ゲームオーバー
        GameOutcomeManager.Instance.RegisterGameOverCallback(StartFadeOut);
        GameOutcomeManager.Instance.RegisterGameOverCallback(StartGameOverUI);

        // ゲームクリア
        GameOutcomeManager.Instance.RegisterGameClearCallback(StartFadeOut);
        GameOutcomeManager.Instance.RegisterGameClearCallback(StartGameClearUI);
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private void StartGameOverUI()
    {
        blackPanel.transform.SetAsFirstSibling();
        StartCoroutine(ShowUIAfterDelay(gameOverUI_));
        Debug.Log("GameOverUI");
    }

    private void StartGameClearUI()
    {
        StartCoroutine(ShowUIAfterDelay(gameClearUI_));
        Debug.Log("GameClearUI");

    }

    private IEnumerator FadeOutCoroutine()
    {
        Input.ResetInputAxes();
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        float t = 0f;
        float duration = animDuration;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; //これ重要：停止中でも動く(停止するとフェードも終わるから)
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            blackPanel.color = c;
            yield return null;
        }

        // ここで全体停止！
        //Time.timeScale = 0f;

        // ここでGameOver UI表示
        //gameOverUI_.SetActive(true);
    }


    private IEnumerator ShowUIAfterDelay(GameObject ui)
    {
        yield return new WaitForSeconds(animDuration);
        if (ui != null)
        {
            ui.SetActive(true);
        }
    }
}
