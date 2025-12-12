using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;

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
    [SerializeField]private Vector2 cursorPos;
    private Vector2 moveInput;

    private PointerEventData eventData;
    private List<RaycastResult> raycastResults;

    private GameObject currentHoverObject = null;

    [SerializeField]Vector2 screenPoint;
    public void InitCursorPos()
    {
        if (cursorRect != null)
        {
            cursorPos = cursorRect.anchoredPosition;
        }
    }
    void Start()
    {
        InitCursorPos();

        eventData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>();

        //Cursor.visible = false;
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

        // 1. カーソル位置の更新
        cursorPos += moveInput * speed * Time.unscaledDeltaTime;

        // 2. カーソル位置を親Canvas内に制限
        ClampCursorToCanvasRect();

        cursorRect.anchoredPosition = cursorPos;

        // 3. ホバーイベント処理
        HandleHoverEvents();

        // 4. クリック処理
        HandleUIClick();
    }

    private void ClampCursorToCanvasRect()
    {
        // 親のCanvasを取得
        Canvas parentCanvas = cursorRect.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        // Canvasの RectTransform を取得
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        if (canvasRect == null) return;

        // Canvas のローカル座標での bounds を計算
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 canvasMin = -canvasSize * canvasRect.pivot;
        Vector2 canvasMax = canvasMin + canvasSize;

        // カーソル位置を制限
        cursorPos.x = Mathf.Clamp(cursorPos.x, canvasMin.x, canvasMax.x);
        cursorPos.y = Mathf.Clamp(cursorPos.y, canvasMin.y, canvasMax.y);


    }

    // カーソルがUI要素に乗った/離れたときのイベント処理 (ハイライトに必須)
    private void HandleHoverEvents()
    {
        // ★ 修正点: UICursorのスクリーン座標を直接イベントデータに設定する ★
        screenPoint = RectTransformUtility.WorldToScreenPoint(null, cursorRect.position);
        eventData.position = screenPoint;

        raycastResults.Clear();

        EventSystem.current.RaycastAll(eventData, raycastResults);

        // --- ホバーしたオブジェクトの判定 ---
        GameObject newHoverObject = null;
        if (raycastResults.Count > 0)
        {
            newHoverObject = raycastResults[0].gameObject;
            Debug.Log("Raycast");
            // Buttonのみ強調表示
            if (newHoverObject.GetComponent<Button>() == null)
            {
                Debug.Log($"Button null{newHoverObject.name}");
                return;
            }
            AddOutline(newHoverObject);
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
  
    private void AddOutline(GameObject obj)
    {
       obj.GetOrAddComponent<OutlineButton>();
    }
}