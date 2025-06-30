using UnityEngine;

/// <summary>
/// 敵の状態enum例
/// </summary>
public enum EnemyState
{
    IDLE,
    PATROL,
    CHASE,
    ATTACK,
    ESCAPE,
    DEAD
}

/// <summary>
/// StateマシンのUnity使用例
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float detectionRange_ = 10f;
    [SerializeField] private float attackRange_ = 2f;
    [SerializeField] private float escapeHealthPercent_ = 0.3f;
    [SerializeField] private float currentHealth_ = 100f;
    [SerializeField] private float maxHealth_ = 100f;

    [Header("デバッグ")]
    [SerializeField] private EnemyState currentState_;

    // ステートマシン
    private StateMachine<EnemyState> stateMachine_;
    
    // プレイヤー参照（例）
    private Transform player_;
    private bool playerInRange_;

    void Start()
    {
        // プレイヤーを探す（例）
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player_ = playerObj.transform;

        // ステートマシンの初期化
        InitializeStateMachine();
        
        // 初期状態をIDLEに設定
        stateMachine_.Start(EnemyState.IDLE);
    }

    void Update()
    {
        // プレイヤーとの距離チェック
        UpdatePlayerDetection();
        
        // ステートマシンの更新
        stateMachine_.UpdateCurrent();
        
        // デバッグ用：現在の状態を表示
        currentState_ = stateMachine_.CurrentState;
    }

    /// <summary>
    /// ステートマシンを初期化
    /// </summary>
    private void InitializeStateMachine()
    {
        stateMachine_ = new StateMachine<EnemyState>();

        // IDLE状態の設定
        stateMachine_.RegisterState(EnemyState.IDLE)
            .SetCallbacks(
                onEntry: () => Debug.Log("敵：待機開始"),
                onUpdate: () => { /* 待機中の処理 */ },
                onExit: () => Debug.Log("敵：待機終了")
            )
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.CHASE, () => playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // PATROL状態の設定
        stateMachine_.RegisterState(EnemyState.PATROL)
            .SetCallbacks(
                onEntry: () => Debug.Log("敵：巡回開始"),
                onUpdate: () => DoPatrol(),
                onExit: () => Debug.Log("敵：巡回終了")
            )
            .AddTransition(EnemyState.CHASE, () => playerInRange_)
            .AddTransition(EnemyState.IDLE, () => Random.Range(0f, 1f) < 0.01f) // 1%の確率で待機
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // CHASE状態の設定
        stateMachine_.RegisterState(EnemyState.CHASE)
            .SetCallbacks(
                onEntry: () => Debug.Log("敵：追跡開始"),
                onUpdate: () => DoChase(),
                onExit: () => Debug.Log("敵：追跡終了")
            )
            .AddTransition(EnemyState.ATTACK, () => GetDistanceToPlayer() <= attackRange_)
            .AddTransition(EnemyState.ESCAPE, () => GetHealthPercent() <= escapeHealthPercent_)
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // ATTACK状態の設定
        stateMachine_.RegisterState(EnemyState.ATTACK)
            .SetCallbacks(
                onEntry: () => Debug.Log("敵：攻撃開始"),
                onUpdate: () => DoAttack(),
                onExit: () => Debug.Log("敵：攻撃終了")
            )
            .AddTransition(EnemyState.CHASE, () => GetDistanceToPlayer() > attackRange_)
            .AddTransition(EnemyState.ESCAPE, () => GetHealthPercent() <= escapeHealthPercent_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // ESCAPE状態の設定
        stateMachine_.RegisterState(EnemyState.ESCAPE)
            .SetCallbacks(
                onEntry: () => Debug.Log("敵：逃走開始"),
                onUpdate: () => DoEscape(),
                onExit: () => Debug.Log("敵：逃走終了")
            )
            .AddTransition(EnemyState.CHASE, () => GetHealthPercent() > escapeHealthPercent_ && playerInRange_)
            .AddTransition(EnemyState.PATROL, () => !playerInRange_)
            .AddTransition(EnemyState.DEAD, () => currentHealth_ <= 0);

        // DEAD状態の設定
        stateMachine_.RegisterState(EnemyState.DEAD)
            .SetCallbacks(
                onEntry: () => {
                    Debug.Log("敵：死亡");
                    // 死亡処理
                    gameObject.SetActive(false);
                },
                onUpdate: () => { /* 死亡状態では何もしない */ },
                onExit: () => { /* 死亡状態からは出ない */ }
            );
    }

    #region 状態別の処理

    private void DoPatrol()
    {
        // 巡回処理
        // transform.Translate(Vector3.forward * Time.deltaTime);
    }

    private void DoChase()
    {
        // 追跡処理
        if (player_ != null)
        {
            Vector3 direction = (player_.position - transform.position).normalized;
            transform.Translate(direction * Time.deltaTime * 3f);
        }
    }

    private void DoAttack()
    {
        // 攻撃処理
        Debug.Log("攻撃中...");
    }

    private void DoEscape()
    {
        // 逃走処理
        if (player_ != null)
        {
            Vector3 direction = (transform.position - player_.position).normalized;
            transform.Translate(direction * Time.deltaTime * 5f);
        }
    }

    #endregion

    #region ヘルパーメソッド

    private void UpdatePlayerDetection()
    {
        if (player_ == null) return;
        
        float distance = GetDistanceToPlayer();
        playerInRange_ = distance <= detectionRange_;
    }

    private float GetDistanceToPlayer()
    {
        if (player_ == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player_.position);
    }

    private float GetHealthPercent()
    {
        return currentHealth_ / maxHealth_;
    }

    #endregion

    #region 公開メソッド

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth_ = Mathf.Max(0, currentHealth_ - damage);
    }

    /// <summary>
    /// 強制的に状態を変更
    /// </summary>
    public void ForceState(EnemyState newState)
    {
        stateMachine_.TransitionTo(newState);
    }

    #endregion

    #region デバッグ

    void OnDrawGizmosSelected()
    {
        // 検出範囲の表示
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange_);
        
        // 攻撃範囲の表示
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange_);
    }

    #endregion
}