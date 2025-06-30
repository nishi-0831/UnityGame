using UnityEngine;

[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize = true;
    //[SerializeField] protected float firstT_ = 0.0f;
    protected SplineController splineController_;
    protected GameObject followTarget_;
    protected bool isActive_ = true;
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
    protected virtual void Awake()
    {
        InitializeComponents();
    }
    
    protected void Start()
    {
        if (autoInitialize)
        {
            if (splineController_ != null)
            {
                //初期位置設定
                splineController_.MoveAlongSpline(FirstT);

                // イベントの登録
                splineController_.onMaxT += OnReachMaxT;
                splineController_.onMinT += OnReachMinT;

                Initialize();
            }
        }
    }
    
    protected virtual void Update()
    {
        if (!isActive_)
        {
            return;
        }
        if (splineController_ != null)
        {
            UpdateMovement();
        }
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
        followTarget_ = gameObject;
        
        
        splineController_.FollowTarget = followTarget_;
    }
    
    /// <summary>
    /// 初期化処理（派生クラスでオーバーライド可能）
    /// </summary>
    protected virtual void Initialize()
    {
        
    }
    
    /// <summary>
    /// 毎フレームの移動処理
    /// </summary>
    protected virtual void UpdateMovement()
    {
        // 基本の移動処理は派生クラスで実装
        OnUpdateMovement();
    }
    
    /// <summary>
    /// t値が1.0を超えた時の処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnReachMaxT()
    {
        Debug.Log($"{gameObject.name}: Reached Max T");
    }
    
    /// <summary>
    /// t値が0.0を下回った時の処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnReachMinT()
    {
        Debug.Log($"{gameObject.name}: Reached Min T");
    }
    
    /// <summary>
    /// 初期化時の処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnInitialize()
    {
        // 派生クラスで実装
    }
    
    /// <summary>
    /// 毎フレームの移動処理（派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnUpdateMovement()
    {
        // 派生クラスで実装
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
}