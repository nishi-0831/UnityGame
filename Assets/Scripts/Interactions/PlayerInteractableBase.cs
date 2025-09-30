using UnityEngine;

/// <summary>
/// 曲線上でプレイヤーと相互作用するオブジェクトの基底クラス
/// アクティブ状態、アニメーション、パラメータを管理
/// <para>OnStompedやOnSideHitをオーバーライドして利用してください</para>
/// </summary>
public abstract class PlayerInteractableBase : SplineMovementBase , IPlayerInteractable
{
    [Header("Player Interaction")]
    [SerializeField] private PlayerInteractionProfile profile;
    [HideInInspector] protected int animIDDie;
    [HideInInspector] protected int animIDAttack;
    [HideInInspector] protected Animator animator;

    /// <summary>
    /// スプライン上の現在位置T値を取得
    /// </summary>
    public float T
    {
        get => splineController_.T;
    }

    /// <summary>
    /// 踏み潰し可能かどうかを取得
    /// </summary>
    public bool CanBeStomped
    {
        get => profile != null && profile.canBeStomped;
        private set { if (profile != null) profile.canBeStomped = value; }
    }

    /// <summary>
    /// プレイヤーに与えるダメージ量を取得
    /// </summary>
    public int DamageToPlayer
    {
        get => profile != null ? profile.damageToPlayer : 0;
        private set { if (profile != null) profile.damageToPlayer = value; }
    }

    /// <summary>
    /// 踏み潰し時のバウンド力を取得
    /// </summary>
    public float StompBounceForce
    {
        get => profile != null ? profile.stompBounceForce : 0f;
        private set { if (profile != null) profile.stompBounceForce = value; }
    }

    ///// <summary>
    ///// 横方向ヒット時のノックバック力を取得
    ///// </summary>
    //public float SideHitKnockbackForce
    //{
    //    get => profile != null ? profile.sideHitKnockbackForce : 0f;
    //    private set { if (profile != null) profile.sideHitKnockbackForce = value; }
    //}

    /// <summary>
    /// アニメーターとアニメーションIDを初期化
    /// </summary>
    protected override void Initialize()
    {
        animator = GetComponent<Animator>();
        animIDDie = Animator.StringToHash("Die");
        animIDAttack = Animator.StringToHash("Attack");
    }

    /// <summary>
    /// インタラクションが有効かどうかを判定
    /// </summary>
    protected virtual bool IsInteractableActive()
    {
        return IsActive_;
    }

    /// <summary>
    /// 踏み潰し時のフラグチェックと処理呼び出し
    /// アクティブな場合のみOnStompedCoreを呼ぶ
    /// </summary>
    public void OnStomped(GameObject player)
    {
        if (!IsInteractableActive()) return;

        OnStompedCore(player);
    }

    /// <summary>
    /// 横方向ヒット時のフラグチェックと処理呼び出し
    /// アクティブな場合のみOnSideHitCoreを呼ぶ
    /// </summary>
    public void OnSideHit(GameObject player)
    {
        if (!IsInteractableActive()) return;

        OnSideHitCore(player);
    }

    /// <summary>
    /// 自分を破棄する
    /// </summary>
    public override void OnRequestDestroy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// ダメージ受信の共通処理
    /// Disableとアニメーション再生を行う
    /// </summary>
    protected void OnDamage()
    {
        Disable();

        animator?.SetTrigger(animIDDie);    
    }

    /// <summary>
    /// 踏み潰し時の固有処理
    /// ドキュメントは派生クラスでオーバーライドして実装
    /// </summary>
    public virtual void OnStompedCore(GameObject player) { }

    /// <summary>
    /// 横方向ヒット時の固有処理
    /// ドキュメントは派生クラスでオーバーライドして実装
    /// </summary>
    public virtual void OnSideHitCore(GameObject player) { }
}