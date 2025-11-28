using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class GamepadCursorController : MonoBehaviour
{
    // インスペクターから設定
    public RectTransform cursorRect; // UICursorのRectTransform
    public float speed = 1000f;     // カーソルの移動速度

    // 現在のカーソル位置
    private Vector2 cursorPos;

    // スティック入力の値
    private Vector2 moveInput;

    void Start()
    {
        // カーソル画像をCanvasの親にする
        if (cursorRect != null)
        {
            cursorPos = cursorRect.anchoredPosition;
        }
        // PCのマウスカーソルを非表示にする（任意）
        Cursor.visible = false;

        // Input SystemのUI/Moveアクションを購読（Listen）
        // ※このコードではUpdate()でGamepad.currentから直接読み込むため不要ですが、
        //   本格的な設計ではInputAction.performedなどで購読します。
    }

    void Update()
    {
        // 1. スティック入力の取得
        if (Gamepad.current != null)
        {
            // 左スティックの値を直接読み込む
            moveInput = Gamepad.current.leftStick.ReadValue();
        }
        else
        {
            moveInput = Vector2.zero;
            return;
        }

        // 2. カーソル位置の更新
        cursorPos += moveInput * speed * Time.deltaTime;

        // 3. カーソル位置を画面内に制限（オプション）
        // TODO: ここに画面端の制限処理を追加すると、より実用的になる

        // 4. UIに反映
        cursorRect.anchoredPosition = cursorPos;

        // 5. ボタンのクリック判定（重要）
        HandleUIClick();
    }

    private void HandleUIClick()
    {
        // 1. Aボタン（Submit）が押された瞬間を判定
        if (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame)
        {
            // 2. EventSystemのポインターデータを作成
            //    カーソルの位置をポインターの位置として扱うために必要
            PointerEventData eventData = new PointerEventData(EventSystem.current);

            // RectTransformの位置（ローカル座標）を画面座標に変換
            // この処理が成功すると、eventData.positionに画面上の座標が入ります
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, cursorRect.position);
            eventData.position = screenPoint;

            // 3. UI Raycasterを使用して、カーソルの下にあるUI要素を検出
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // 4. 検出された要素をチェック
            foreach (RaycastResult result in results)
            {
                // RaycastResultがヒットしたオブジェクトを取得
                GameObject hitObject = result.gameObject;

                // ボタンコンポーネントがあるかチェック
                Button button = hitObject.GetComponent<Button>();
                if (button != null && button.interactable)
                {
                    // 5. ボタンのクリックイベントを発火させる
                    Debug.Log($"Clicked: {hitObject.name}");
                    button.onClick.Invoke();

                    // 目的のボタンをクリックしたら処理を終了
                    return;
                }
            }
        }
    }
}