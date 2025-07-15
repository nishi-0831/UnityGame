
using UnityEngine;


[RequireComponent(typeof(SplineController))]
public abstract class SplineMovementBase : MonoBehaviour
{
    [Header("Spline Movement Settings")]
    [SerializeField] protected int hp_ = 1;
    [SerializeField] protected float speed_ = 1.0f;
    [SerializeField] protected bool autoInitialize_ = true;
    [SerializeField] protected string wallTag_ = "Wall";
    [SerializeField]protected SplineController splineController_;
    [SerializeField]protected bool isActive_ = true;
    protected Collider targetCollider_;

    [Header("レイヤーの設定")]
    [SerializeField] protected SplineLayerSettings layerSettings_;
    [Header("OnDestroyで再生されるSE")]
    [SerializeField] private AudioClip onDestroySE_ = null;

    public GameObject FollowTarget
    {
        get { return splineController_.FollowTarget; }
        set { splineController_.FollowTarget = value; }
    }

    //public bool isOffSpline_
    //{
    //    get { return splineController_.isOffSpline_; }
    //    set { splineController_.isOffSpline_ = value; }
    //}

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
    
    protected virtual  void Start()
    {
        if (autoInitialize_)
        {
            if (splineController_ != null)
            {
                //初期位置設定
                splineController_.MoveAlongSpline(splineController_.T);

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
    /// WallTagのColliderに衝突した時の処理 (派生クラスでオーバーライド）
    /// </summary>
    protected virtual void OnCollideWall()
    {
        //Debug.Log($"{gameObject.name}: Collide Wall");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(wallTag_))
        {
            OnCollideWall();
        }
    }
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.CompareTag(wallTag_))
    //    {
    //        OnCollideWall();
    //    }
    //    Debug.Log(other.gameObject.name);
    //}
    

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
    /// 破棄要請が来たとき
    /// </summary>
    public virtual void OnRequestDestroy()
    {
        Debug.Log($"{gameObject.name} : was requested destroy");
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
    /// 曲線上の移動、当たり判定を有効化
    /// </summary>
    protected void Enable()
    {
        Debug.Log($"Enable:{FollowTarget.name}");
        IsActive_ = true;
        FollowTarget.layer = (int)Mathf.Log(layerSettings_.activeLayer.value, 2);
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
        if(onDestroySE_ != null)
        {
            AudioSource.PlayClipAtPoint(onDestroySE_, FollowTarget.transform.position);
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

    public virtual void OnDamage(int damageValue, float enemyT)
    {

    }
    public virtual void OnDamage(int damageValue,Vector3 enemyPos)
    { }
}