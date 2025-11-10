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
    protected PlayerInteractionProfile Profile
    {
        get
        {
            if (profile == null)
            {
                // クラス名を取得
                string className = GetType().Name;
                profile = Resources.Load<PlayerInteractionProfile>($"PlayerInteractionProfiles/{className}");

                if(profile == null)
                {
                    Debug.LogWarning($"{className} という名前のPlayerInteractionProfileが見当たりませんでした。" +
                                  $"Resources/PlayerInteractionProfiles/{className}のように作ってください");
                    // フォールバック用のデフォルトの設定を読み込む
                    profile = Resources.Load<PlayerInteractionProfile>("PlayerInteractionProfiles/Default");
                }

                

                if (profile == null)
                {
                    Debug.LogError("Resources/PlayerInteractionProfiles/Default が見当たりませんでした");
                }
            }
            return profile;
        }
    }
    [HideInInspector] protected int animIDDie;
    [HideInInspector] protected int animIDAttack;
    [HideInInspector] protected Animator animator;

    /// <summary>
    /// スプライン上の現在位値を取得
    /// </summary>
    public float Progress
    {
        get => splineController_.Progress;
    }

    /// <summary>
    /// 踏み潰し可能かどうかを取得
    /// </summary>
    public bool CanBeStomped
    {
        get => Profile != null && Profile.canBeStomped;
        private set { if (Profile != null) Profile.canBeStomped = value; }
    }

    /// <summary>
    /// プレイヤーに与えるダメージ量を取得
    /// </summary>
    public int DamageToPlayer
    {
        get => Profile != null ? Profile.damageToPlayer : 0;
        private set { if (Profile != null) Profile.damageToPlayer = value; }
    }

    /// <summary>
    /// 踏みつけ時にプレイヤーに加えられる+Y方向の力
    /// </summary>
    public float StompBounceForce
    {
        get => Profile != null ? Profile.stompBounceForce : 0f;
        private set { if (Profile != null) Profile.stompBounceForce = value; }
    }

    /// <summary>
    /// 撃破時のスコア加算値
    /// </summary>
    public int ScoreValue
    {
        get => Profile != null ? Profile.scoreValue : 0;
        private set { if (Profile != null) Profile.stompBounceForce = value; }
    }

    ///// <summary>
    ///// 横方向ヒット時のノックバック力を取得
    ///// </summary>
    //public float SideHitKnockbackForce
    //{
    //    get => Profile != null ? Profile.sideHitKnockbackForce : 0f;
    //    private set { if (Profile != null) Profile.sideHitKnockbackForce = value; }
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
        if (ScoreManager.Instance)
        {
            ScoreManager.Instance.ReceiveScore(ScoreValue);
        }
        if (animator)
        {
            animator.SetTrigger(animIDDie);
        }

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