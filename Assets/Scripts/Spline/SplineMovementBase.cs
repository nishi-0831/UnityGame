using UnityEngine;


[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected int hp_ = 1;
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize_ = true;
    
    [SerializeField]protected SplineController splineController_;
    protected bool isActive_ = true;
    protected Collider targetCollider_;

    [Header("レイヤーの設定")]
    [SerializeField] protected SplineLayerSettings layerSettings_;
    
    public GameObject FollowTarget
    {
        get { return splineController_.FollowTarget; }
        set { splineController_.FollowTarget = value; }
    }
    public bool IsActive_
    {
        get { return isActive_; }
        protected set { isActive_ = value; }
    }
    public bool IsMovingLeft 
    {
        get { return splineController_.isMovingLeft; } 
        protected set { splineController_.isMovingLeft = value; }
    }

    public float FirstT
    {
        get { return splineController_.FirstT; }
        protected set
        {
            if (splineController_ != null)
            {
                splineController_.FirstT = value;
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if(splineController_ == null)
        {
            splineController_ = GetComponent<SplineController>();
        }
    }
#endif
    private void Awake()
    {
        InitializeComponents();
        Initialize();
    }
    
    private  void Start()
    {
        if (autoInitialize_)
        {
            if (splineController_ != null)
            {
                //初期位置設定
                splineController_.MoveAlongSpline(FirstT);

                // イベントの登録
                splineController_.onMaxT += OnReachMaxT;
                splineController_.onMinT += OnReachMinT;
            }
        }
    }
    
    private void Update()
    {
        if (!isActive_)
        {
            return;
        }

        UpdateMovement();
    }
    
    /// <summary>
    /// 必要なコンポーネントを自動取得
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
        //includelayer等を設定...
        //if(layerSettings_ != null)
        {
            targetCollider_.excludeLayers = layerSettings_.disabledLayer;

            FollowTarget.layer = (int)Mathf.Log(layerSettings_.activeLayer.value, 2);
            
        }

        
        
    }
   
   

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected virtual void Initialize()
    {

    }

    /// <summary>
    /// 毎フレームの移動処理
    /// </summary>
    protected virtual void UpdateMovement()
    {
       
    }
    
    /// <summary>
    /// t値が1.0を超えた時の処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnReachMaxT()
    {
        //Debug.Log($"{gameObject.name}: Reached Max T");
    }
    
    /// <summary>
    /// t値が0.0を下回った時の処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnReachMinT()
    {
        //Debug.Log($"{gameObject.name}: Reached Min T");
    }
   
    
    protected void CancelOnReachMinT()
    {
        splineController_.onMinT -= OnReachMinT;
    }

    protected void CancelOnReachMaxT()
    {
        splineController_.onMaxT -= OnReachMaxT;
    }

    /// <summary>
    /// 曲線上の移動、当たり判定を無効化
    /// </summary>
    protected void Disable()
    {
        Debug.Log($"Disable:{FollowTarget.name}");
        IsActive_ = false;
        FollowTarget.layer = (int)Mathf.Log(layerSettings_.disabledLayer.value, 2);
    }

    /// <summary>
    /// 破棄時の処理
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
    /// ダメージを受けた時の処理
    /// </summary>
    public virtual void OnDamage()
    {
        Debug.Log($"{FollowTarget.name}がダメージを食らった");
    }
    public virtual void OnDamage(int damageValue)
    {
        Debug.Log($"{FollowTarget.name}が{damageValue}のダメージを受けた");
    }
}