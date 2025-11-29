using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingScoreManager : MonoBehaviour
{
    public static FloatingScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI floatingScorePrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float floatingDuration = 1.0f;
    [SerializeField] private float floatingHeight = 1.5f;
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisplayFloatingScore(int scoreValue,Vector3 worldPositon)
    {
        if(floatingScorePrefab == null || canvas == null)
        {
            Debug.LogError("Prefab or Canvas is not assigned");
            return;
        }

        // Prefabをインスタンス化する
        TextMeshProUGUI floatingText = Instantiate(floatingScorePrefab, canvas.transform);
        floatingText.text = $"+{scoreValue}";

        // ワールド座標系をスクリーン座標系に変換
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPositon);
        // スクリーン座標系をCanvasの座標系に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.worldCamera,
            out Vector2 uiPosition);

        // Canvasの座標系を設定
        floatingText.GetComponent<RectTransform>().anchoredPosition = uiPosition;
        // アニメーション開始
        StartCoroutine(FloatingScoreAnimation(floatingText));
    }

    private IEnumerator FloatingScoreAnimation(TextMeshProUGUI floatingText)
    {
        RectTransform rectTransform = floatingText.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        Color startColor = floatingText.color;
        float elapsedTime = 0f;

        while (elapsedTime < floatingDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / floatingDuration;

            // Y方向に移動
            Vector2 newPosition = startPosition;
            newPosition.y = floatingHeight * progress;
            rectTransform.anchoredPosition = newPosition;

            // フェードアウト
            Color newColor = startColor;
            newColor.a = alphaCurve.Evaluate(progress);
            floatingText.color = newColor;

            yield return null;
        }

        // テキストを破棄
        Destroy(floatingText.gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
