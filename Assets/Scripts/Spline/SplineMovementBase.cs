using UnityEngine;


/// <summary>
/// <para>- 曲線上を動くオブジェクトの基底クラス</para>
/// <para>- SplineController を介して移動やイベントを制御する</para>
/// </summary>
[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    //[Header("Spline Movement Settings")]
    /// <summary>
    /// <para>- 体力の初期値</para>
    /// <para>- 0 以下で破棄や無効化のトリガーに使う</para>
    /// </summary>
    //[SerializeField] protected int hp_ = 1;

    /// <summary>
    /// <para>- 基本移動速度</para>
    /// <para>- SplineController の更新処理で参照する</para>
    /// </summary>
    [SerializeField] protected float speed_ = 1.0f;

    [Header("インスタンス時に自動で初期化を行うか")]
    /// <summary>
    /// <para>- Awake でのコンポーネント初期化後に Start で初期化処理を自動実行するかを切り替える</para>
    /// </summary>
    [SerializeField] protected bool autoInitialize_ = true;


    [Header("曲線上を動くために使うクラス")]
    /// <summary>
    /// <para>- スプライン上の移動を制御するコンポーネント</para>
    /// <para>- フォロー対象や t 値の更新を管理する</para>
    /// </summary>
    [SerializeField] public SplineController splineController_;

    [Header("他のSplineMovementBase派生クラスと相互作用を行うか否か")]
    /// <summary>
    /// <para>- このオブジェクトが相互作用や移動を行うかのフラグ</para>
    /// <para>- UpdateMovement の実行可否に影響する</para>
    /// </summary>
    [SerializeField] protected bool isActive_ = true;

    [Header("このクラスが扱う対象とするコライダー")]
    /// <summary>
    /// <para>- 操作対象のコライダー</para>
    /// <para>- フォロー対象から取得する</para>
    /// </summary>
    protected Collider targetCollider_;

    [Header("当たり判定の有無を規定するレイヤーの設定")]
    /// <summary>
    /// <para>- レイヤーの有効無効や地面レイヤーをまとめた設定</para>
    /// <para>- フォロー対象のレイヤーや除外レイヤーに反映する</para>
    /// </summary>
    public SplineLayerSettings LayerSettings
    {
        get { return splineController_.SplineLayerSettings; }

    }
    /// <summary>
    /// <para>- SplineController が参照するフォロー対象</para>
    /// <para>- setter で SplineController 側へ反映する</para>
    /// </summary>
    public GameObject FollowTarget
    {
        get { return splineController_.FollowTarget; }
        set { splineController_.FollowTarget = value; }
    }

    public float Progress
    {
        get { return splineController_.Progress; }
    }
    /// <summary>
    /// <para>- このオブジェクトが有効かどうか</para>
    /// <para>- 移動や相互作用の可否を表す</para>
    /// </summary>
    public bool IsActive_
    {
        get { return isActive_; }
        protected set { isActive_ = value; }
    }

    /// <summary>
    /// <para>- 左方向へ移動しているかどうか</para>
    /// <para>- SplineController の isMovingLeft を参照する</para>
    /// </summary>
    public bool IsMovingLeft 
    {
        get { return splineController_.isMovingLeft; } 
        protected set { splineController_.isMovingLeft = value; }
    }

 
#if UNITY_EDITOR
    /// <summary>
    /// <para>- エディタ上でインスペクタ値が変更された際に参照の整合性を取る</para>
    /// <para>- splineController_ が未設定なら自動取得する</para>
    /// </summary>
    private void OnValidate()
    {
        if(splineController_ == null)
        {
            splineController_ = GetComponent<SplineController>();
        }
    }
#endif
    /// <summary>
    /// <para>- コンポーネントの初期化を行う</para>
    /// <para>- SplineController とコライダー設定を準備する</para>
    /// </summary>
    private void Awake()
    {
        InitializeComponents();
        Initialize();
    }
    
    /// <summary>
    /// <para>- 自動初期化が有効な場合に移動の初期化とイベント登録を行う</para>
    /// <para>- t 値に基づき初期位置へ移動する</para>
    /// </summary>
    protected virtual  void Start()
    {
        if (autoInitialize_)
        {
            if (splineController_ != null)
            {
                //初期位置設定
                splineController_.MoveAlongSpline(splineController_.Progress);

                // イベントの登録
                splineController_.onMaxT += OnReachMaxT;
                splineController_.onMinT += OnReachMinT;
            }
        }
    }
    
    /// <summary>
    /// <para>- フレーム毎の移動更新を行う</para>
    /// <para>- isActive_ が false の場合はスキップする</para>
    /// </summary>
    private void Update()
    {
        if (!isActive_)
        {
            return;
        }

        UpdateMovement();
    }
    
    /// <summary>
    /// <para>- 必要なコンポーネントと参照を自動取得する</para>
    /// <para>- フォロー対象やコライダーの層設定を反映する</para>
    /// </summary>
    protected virtual void InitializeComponents()
    {
        splineController_ = GetComponent<SplineController>();
        if (splineController_ == null)
        {
            Debug.LogError($"{gameObject.name}: SplineController component not found!");
            return;
        }
        
        //自身をfollowTargetとして使用
        FollowTarget = gameObject;
        targetCollider_ = FollowTarget.GetComponent<Collider>();
        targetCollider_.isTrigger = true;
        //includelayer等を設定...
        //if(LayerSettings != null)
        {
            targetCollider_.excludeLayers = LayerSettings.disabledLayer;

            FollowTarget.layer = (int)Mathf.Log(LayerSettings.activeLayer.value, 2);
        }
    }
   
   

    /// <summary>
    /// <para>- 派生先で必要な初期化を行うためのフック</para>
    /// <para>- Awake の最後に呼ばれる</para>
    /// </summary>
    protected virtual void Initialize()
    {

    }

    /// <summary>
    /// <para>- 派生先でフレーム毎の移動処理を実装するためのフック</para>
    /// </summary>
    protected virtual void UpdateMovement()
    {
       
    }

    /// <summary>
    /// <para>- 壁タグのコライダーに衝突した際の処理</para>
    /// <para>- 派生クラスで上書きして利用する</para>
    /// </summary>
    protected virtual void OnCollideWall()
    {
        //Debug.Log($"{gameObject.name}: Collide Wall");
    }

    /// <summary>
    /// <para>- トリガーに入った際に壁タグを検知して OnCollideWall を呼ぶ</para>
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (int)Mathf.Log(LayerSettings.groundLayer,2))
        {
            OnCollideWall();
        }
    }
    
    

    /// <summary>
    /// <para>- t 値が 1.0 を超えた時の処理</para>
    /// <para>- 派生クラスで上書きして利用する</para>
    /// </summary>
    protected virtual void OnReachMaxT()
    {
        Debug.Log($"{gameObject.name}: Reached Max Progress");
    }
    
    /// <summary>
    /// <para>- t 値が 0.0 を下回った時の処理</para>
    /// <para>- 派生クラスで上書きして利用する</para>
    /// </summary>
    protected virtual void OnReachMinT()
    {
        Debug.Log($"{gameObject.name}: Reached Min Progress");
    }
   
    /// <summary>
    /// <para>- 破棄要請を受けた時のコールバック</para>
    /// <para>- 派生クラスで上書きして破棄演出などを行う</para>
    /// </summary>
    public virtual void OnRequestDestroy()
    {
        Debug.Log($"{gameObject.name} : was requested destroy");
    }

    /// <summary>
    /// <para>- onMinT 登録を解除する</para>
    /// </summary>
    protected void CancelOnReachMinT()
    {
        splineController_.onMinT -= OnReachMinT;
    }

    /// <summary>
    /// <para>- onMaxT 登録を解除する</para>
    /// </summary>
    protected void CancelOnReachMaxT()
    {
        splineController_.onMaxT -= OnReachMaxT;
    }

    /// <summary>
    /// <para>- 曲線上の移動と当たり判定を無効化する</para>
    /// <para>- フォロー対象のレイヤーを無効レイヤーへ切り替える</para>
    /// </summary>
    protected void Disable()
    {
        Debug.Log($"Disable:{FollowTarget.name}");
        IsActive_ = false;
        FollowTarget.layer = (int)Mathf.Log(LayerSettings.disabledLayer.value, 2);
    }

    /// <summary>
    /// <para>- 曲線上の移動と当たり判定を有効化する</para>
    /// <para>- フォロー対象のレイヤーを有効レイヤーへ切り替える</para>
    /// </summary>
    protected void Enable()
    {
        Debug.Log($"Enable:{FollowTarget.name}");
        IsActive_ = true;
        FollowTarget.layer = (int)Mathf.Log(LayerSettings.activeLayer.value, 2);
    }

    /// <summary>
    /// <para>- 破棄時の後処理を行う</para>
    /// <para>- イベントの登録解除と効果音の再生を行う</para>
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (splineController_ != null)
        {
            splineController_.onMaxT -= OnReachMaxT;
            splineController_.onMinT -= OnReachMinT;
        }
    }

    /// <summary>
    /// <para>- ダメージを受けた時の処理</para>
    /// <para>- デフォルト実装はログ出力のみ</para>
    /// </summary>
    //public virtual void OnDamage()
    //{
    //    Debug.Log($"{FollowTarget.name}がダメージを食らった");
    //}

    ///// <summary>
    ///// <para>- 指定値のダメージを受けた時の処理</para>
    ///// <para>- デフォルト実装はログ出力のみ</para>
    ///// </summary>
    //public virtual void OnDamage(int damageValue)
    //{
    //    Debug.Log($"{FollowTarget.name}が{damageValue}のダメージを受けた");
    //}

    ///// <summary>
    ///// <para>- 指定値のダメージと敵の t 値を伴う処理</para>
    ///// <para>- 派生クラスで利用する</para>
    ///// </summary>
    //public virtual void OnDamage(int damageValue, float enemyT)
    //{

    //}

    ///// <summary>
    ///// <para>- 指定値のダメージと敵の座標を伴う処理</para>
    ///// <para>- 派生クラスで利用する</para>
    ///// </summary>
    //public virtual void OnDamage(int damageValue,Vector3 enemyPos)
    //{ }
}