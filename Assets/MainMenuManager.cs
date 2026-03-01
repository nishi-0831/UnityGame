using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ★ 追加
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Button firstSelectedButton;
    // Input Map制御用のフィールドは一旦削除（InputSystemの実装が不明なため）

    private void OnEnable()
    {
        // ★ SceneLoaded イベントにメソッドを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ★ オブジェクトが無効化されるとき、イベントの登録を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンがロード完了したときに呼ばれるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // MainMenuに戻ってきたことを確認するために、シーン名でチェックすることも可能です
        if (scene.name == "MainMenu")
        {
            // コルーチンを使って次のフレームで選択を強制
            StartCoroutine(SelectInitialButtonCoroutine());
        }
    }

    // Start() や OnEnable() から直接呼ばず、イベント経由で実行する
    private IEnumerator SelectInitialButtonCoroutine()
    {
        // 2フレーム待機して、確実にEventSystemの初期化完了を待つ
        yield return null;
        yield return null;

        if (firstSelectedButton == null || EventSystem.current == null)
        {
            Debug.LogError("MainMenuの初期選択ボタンまたはEventSystemが見つかりません。");
            yield break;
        }

        // 選択を強制
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);

        Debug.Log("SceneLoadedイベント経由でMainMenuの初期選択を強制しました。");
    }
}