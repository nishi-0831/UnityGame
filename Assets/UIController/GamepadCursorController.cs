using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;

public class GamepadCursorController : MonoBehaviour
{
    // ★★★ CS1593対策: 静的なデリゲートを定義する ★★★
    private static ExecuteEvents.EventFunction<IPointerEnterHandler> s_PointerEnterHandler = ExecuteEvents_PointerEnterHandler;
    private static ExecuteEvents.EventFunction<IPointerExitHandler> s_PointerExitHandler = ExecuteEvents_PointerExitHandler;

    // Static Delegate Body (イベントの実体処理)
    private static void ExecuteEvents_PointerEnterHandler(IPointerEnterHandler handler, BaseEventData eventData)
    {
        handler.OnPointerEnter((PointerEventData)eventData);
    }
    private static void ExecuteEvents_PointerExitHandler(IPointerExitHandler handler, BaseEventData eventData)
    {
        handler.OnPointerExit((PointerEventData)eventData);
    }

    // === インスペクターで設定する項目 ===
    public RectTransform cursorRect;
    public InputActionReference moveAction;
    public float speed = 1000f;

    // === 内部で使用する変数 ===
    private Vector2 cursorPos;
    private Vector2 moveInput;

    private PointerEventData eventData;
    private List<RaycastResult> raycastResults;

    private GameObject currentHoverObject = null;


    void Start()
    {
        if (cursorRect != null)
        {
            cursorPos = cursorRect.anchoredPosition;
        }

        eventData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        if (moveAction != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
        }
    }

    void OnDestroy()
    {
        if (moveAction != null && moveAction.action.enabled)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }


    void Update()
    {
        if (cursorRect == null) return;

        // 1. カーソル位置の更新と制限
        cursorPos += moveInput * speed * Time.deltaTime;

        float maxX = 900f;
        float maxY = 500f;

        cursorPos.x = Mathf.Clamp(cursorPos.x, -maxX, maxX);
        cursorPos.y = Mathf.Clamp(cursorPos.y, -maxY, maxY);

        cursorRect.anchoredPosition = cursorPos;

        // 2. マウスの強制同期処理は削除。カーソル位置を直接使ってイベントを処理
        HandleHoverEvents();

        // 3. クリック処理
        HandleUIClick();
    }


    // カーソルがUI要素に乗った/離れたときのイベント処理 (ハイライトに必須)
    private void HandleHoverEvents()
    {
        // ★ 修正点: UICursorのスクリーン座標を直接イベントデータに設定する ★
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, cursorRect.position);
        eventData.position = screenPoint;

        raycastResults.Clear();

        EventSystem.current.RaycastAll(eventData, raycastResults);

        // --- ホバーしたオブジェクトの判定 ---
        GameObject newHoverObject = null;
        if (raycastResults.Count > 0)
        {
            newHoverObject = raycastResults[0].gameObject;
        }

        if (newHoverObject != currentHoverObject)
        {
            // 以前ホバーしていたオブジェクトから離れたときの処理 (Pointer Exit)
            if (currentHoverObject != null)
            {
                // ★★★ 修正: 定義した静的デリゲート変数を使用 ★★★
                ExecuteEvents.Execute(currentHoverObject, eventData, s_PointerExitHandler);
            }

            // 新しいオブジェクトにホバーしたときの処理 (Pointer Enter)
            if (newHoverObject != null)
            {
                // ★★★ 修正: 定義した静的デリゲート変数を使用 ★★★
                ExecuteEvents.Execute(newHoverObject, eventData, s_PointerEnterHandler);
            }

            currentHoverObject = newHoverObject;
        }
    }


    // Aボタンが押されたときのクリック処理
    private void HandleUIClick()
    {
        if (Gamepad.current != null && Gamepad.current.aButton.wasPressedThisFrame && currentHoverObject != null)
        {
            Button button = currentHoverObject.GetComponent<Button>();

            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                Debug.Log($"Clicked: {currentHoverObject.name}");
            }
        }
    }
}