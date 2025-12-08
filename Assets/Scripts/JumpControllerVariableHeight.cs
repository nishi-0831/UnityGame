using UnityEngine;

/// <summary>
/// ボタン押下時間でジャンプ高さが変化するコントローラ。
/// </summary>
public class JumpControllerVariableHeight : MonoBehaviour
{
    [Header("基本パラメータ")]
    [Tooltip("最小ジャンプ到達高さ（軽くタップ）")]
    public float minHeight = 1.2f;
    [Tooltip("最大ジャンプ到達高さ（最大ホールド）")]
    public float maxHeight = 4.0f;

    [Tooltip("重力加速度の大きさ（正の値）。例: 9.81 ～ 30 など。値を大きくすると全体が速く短くなる")]
    public float gravityMagnitude = 18f;

    [Tooltip("最大ホールド判定時間（この秒数以降はこれ以上高くならない）")]
    public float maxHoldTime = 0.20f;

    [Tooltip("ボタンを離した後、まだ上昇中なら適用する追加重力倍率。1=無効")]
    public float releaseGravityMultiplier = 2.0f;
    [Tooltip("下降中に適用する重力倍率。1=無効")]
    public float fallGravityMultiplier = 1.5f;

    [Header("その他調整")]
    [Tooltip("離した瞬間に正確な目標高さへ向けて速度再計算（方針A）。false にすると倍率方式のみ。")]
    public bool useVelocityClampOnRelease = true;

    [Tooltip("地面判定許容量（数値誤差対策）")]
    public float groundEpsilon = 0.001f;

    [Tooltip("デバッグ表示")]
    public bool showDebug = false;

    // 状態
    [SerializeField]
    private bool isJumping = false;
    private bool isHolding = false;
    private bool isStompJump = false; // 踏みつけジャンプかどうか
    private float holdTime = 0f;
    private float relativeY = 0f;   // 相対 Y 高さ
    private float velocityY = 0f;   // 上向き正
    private float startPlatformY = 0f;

    // 事後参照用（任意）
    private float targetHeightChosen = 0f;

    /// <summary>
    /// ジャンプ開始。現在の足場位置を起点 0 として相対高度計算。
    /// </summary>
    public void StartJump(float platformY, float customMinHeight = -1f, float customMaxHeight = -1f)
    {
        if (isJumping) return;

        if (customMinHeight > 0f) minHeight = customMinHeight;
        if (customMaxHeight > 0f) maxHeight = customMaxHeight;

        startPlatformY = platformY;
        isJumping = true;
        isHolding = true;
        isStompJump = false; // 通常ジャンプ
        holdTime = 0f;
        relativeY = 0f;

        // 最大高さを前提とした初速度
        // v0 = sqrt(2 g H)
        velocityY = Mathf.Sqrt(2f * gravityMagnitude * Mathf.Max(0.0001f, maxHeight));
        targetHeightChosen = maxHeight; // 一旦最大（途中で下げ得る）
    }

    void Update()
    {
        if(isHolding)
        {
            HoldUpdate();
        }
    }
    /// <summary>
    /// ボタンが押され続けている間呼ぶ（押しっぱなし判定用に Update で監視）。
    /// ここでは時間を積むだけ。高度制御は Release 時。
    /// </summary>
    public void HoldUpdate()
    {
        if (isJumping && isHolding)
        {
            holdTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// ボタン離し。ここで実際に“最終到達高さ”を決定して速度を調整。
    /// </summary>
    public void Release()
    {
        if (!isJumping || !isHolding) return;
        isHolding = false;

        // ホールド率
        float holdFraction = Mathf.Clamp01(holdTime / Mathf.Max(0.0001f, maxHoldTime));
        float target = Mathf.Lerp(minHeight, maxHeight, holdFraction);
        targetHeightChosen = target;

        if (!useVelocityClampOnRelease) return;

        // 既に現在相対高さが目標を超えていれば上昇終了
        if (relativeY >= target)
        {
            if (velocityY > 0f) velocityY = 0f;
            return;
        }

        // 残り上昇して欲しい高さ
        float remaining = target - relativeY;
        if (remaining <= 0f)
        {
            velocityY = 0f;
            return;
        }

        // 目標高さに丁度到達して０速度になるために必要な速度
        float neededVy = Mathf.Sqrt(2f * gravityMagnitude * remaining);

        // 今の速度がそれより大きい場合は削る
        if (velocityY > neededVy)
        {
            velocityY = neededVy;
        }
        // 逆に既に小さすぎるなら放置（より低いジャンプになる）
    }

    /// <summary>
    /// 毎フレーム呼び出し。platformPosition は「いまの足場 / 経路」のワールド座標。
    /// </summary>
    public void Tick(Vector3 platformPosition)
    {
        float deltaTime = Time.deltaTime;
        if (!isJumping)
        {
            // 非ジャンプ時も足場追従
            transform.position = platformPosition;
            return;
        }

        // 実効重力計算
        float g = gravityMagnitude;

        // 上昇中?
        bool ascending = velocityY > 0f;

        // ボタン離し後に上昇中なら追加重力
        if (!isHolding && ascending && releaseGravityMultiplier > 1f && !useVelocityClampOnRelease)
        {
            g *= releaseGravityMultiplier;
        }

        // 下降時
        if (!ascending && fallGravityMultiplier > 1f)
        {
            g *= fallGravityMultiplier;
        }

        // 速度 & 位置更新
        velocityY -= g * deltaTime;
        relativeY += velocityY * deltaTime;

        // 地面（相対 0）以下なら着地
        if (relativeY <= groundEpsilon)
        {
            relativeY = 0f;
            isJumping = false;
            isStompJump = false; // フラグもリセット
            velocityY = 0f;
            isHolding = false;
        }

        // 最終的なワールド座標
        transform.position = new Vector3(
            platformPosition.x,
            platformPosition.y + relativeY,
            platformPosition.z
        );

        if (showDebug)
        {
            Debug.DrawLine(platformPosition, transform.position, ascending ? Color.cyan : Color.magenta);
        }
    }

    // 公開プロパティ
    public bool IsJumping => isJumping;
    public bool IsHolding => isHolding;
    public bool IsStompJump => isStompJump; // 踏みつけジャンプ判定
    public float CurrentRelativeY => relativeY;
    public float CurrentVelocityY => velocityY;
    public float ChosenTargetHeight => targetHeightChosen;

    /// <summary>
    /// 足場（プラットフォーム）を跨いで移動した際に、現在のワールド高さを保ちつつ
    /// 新しい足場の相対高さに自然に変換する。
    /// 例: oldY=0, relativeY=8 の状態で newY=5 に移行 -> relativeY は 3 になる。
    /// </summary>
    /// <param name="oldPlatformY">移行前の足場のY座標</param>
    /// <param name="newPlatformY">移行後の足場のY座標</param>
    public void AdjustForPlatformChange(float oldPlatformY, float newPlatformY)
    {
        // 現在のワールド高さを保持
        float currentWorldY = oldPlatformY + relativeY;

        // 新しい足場基準の相対高さへ変換
        float newRelativeY = currentWorldY - newPlatformY;

        // 内部状態を更新
        startPlatformY = newPlatformY;
        relativeY = newRelativeY;

        // 目標高さの記録値も相対的に再計算（デバッグ用途）
        float deltaBase = newPlatformY - oldPlatformY;
        targetHeightChosen = Mathf.Max(0f, targetHeightChosen - deltaBase);

        // 地面判定（新しい足場基準で地面以下なら着地扱い）
        if (relativeY <= groundEpsilon)
        {
            relativeY = 0f;
            isJumping = false;
            isHolding = false;
            isStompJump = false;
            velocityY = 0f;
        }
        else
        {
            isJumping = true;
        }
        // velocityY はワールド基準の鉛直速度なので基本的に維持する。
    }

    /// <summary>
    /// 敵踏みつけ専用のジャンプ開始。現在の足場位置を起点 0 として相対高度計算。
    /// </summary>
    public void StartStompJump(float platformY, float stompBounceHeight)
    {
        // 既存のジャンプ状態をリセット
        if (isJumping)
        {
            // 現在のジャンプを強制終了
            isJumping = false;
            isHolding = false;
        }

        startPlatformY = platformY;
        isJumping = true;
        isHolding = false; // 踏みつけジャンプはホールド無し
        isStompJump = true; // 踏みつけジャンプフラグ
        holdTime = 0f;
        relativeY = 0f;

        // 踏みつけ専用の高さで初速度を計算
        // v0 = sqrt(2 g H)
        velocityY = Mathf.Sqrt(2f * gravityMagnitude * Mathf.Max(0.0001f, stompBounceHeight));
        targetHeightChosen = stompBounceHeight;

        if (showDebug)
        {
            Debug.Log($"Stomp jump started: height={stompBounceHeight}, velocity={velocityY}");
        }
    }
}