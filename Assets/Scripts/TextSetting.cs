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
        //ゲームオーバー時にフェードアウトとテキスト表示を登録
        playerController.RegisterGameOverCallBack(StartFadeOut);
        playerController.RegisterGameOverCallBack(StartGameOverUI);

        //ゲームクリア時も同様に登録
        playerController.RegisterGameClearCallBack(StartFadeOut);
        playerController.RegisterGameClearCallBack(StartGameClearUI);
    }

    private void StartFadeOut()
    {
        blackPanel.transform.SetAsFirstSibling();
        StartCoroutine(FadeOutCoroutine());
    }

    private void StartGameOverUI()
    {
        StartCoroutine(ShowUIAfterDelay(gameOverUI_));
    }

    private void StartGameClearUI()
    {
        StartCoroutine(ShowUIAfterDelay(gameClearUI_));
    }

    private IEnumerator FadeOutCoroutine()
    {
        blackPanel.gameObject.SetActive(true);
        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        float t = 0f;
        float duration = 1f;

        //フェードアウト（黒くなる）
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            blackPanel.color = c;
            yield return null;
        }

    }


    private IEnumerator ShowUIAfterDelay(GameObject ui)
    {
        //フェードアウトが完了するまで待つ
        yield return new WaitForSeconds(1f);
        if (ui != null)
        {
            ui.SetActive(true);
        }
    }
}
