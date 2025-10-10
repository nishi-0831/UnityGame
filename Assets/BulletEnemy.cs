using UnityEngine;
[RequireComponent(typeof(EaseInterpolator))]

/// <summary>
/// BulletEnemyの説明をここに記述
/// </summary>
public class BulletEnemy : PlayerInteractableBase
{
    public GameObject Sphere;
    public GameObject targetPlayer;
    [SerializeField] private float attackInterval_ = 5.0f;
    [SerializeField] private EaseInterpolator easeInterpolator_;
    float bulletSpeed_ = 200.0f;
    // 独自のフィールドをここに追加
    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        // 初期化処理をここに記述
        easeInterpolator_ = GetComponent<EaseInterpolator>();
        easeInterpolator_.onFinished_ += GenerateBullet;
        easeInterpolator_.Reset();
        easeInterpolator_.duration = attackInterval_;
    }

    /// <summary>
    /// 開始処理（MonoBehaviourのStart相当）
    /// </summary>
    override protected void Start()
    {
        base.Start();
        // 開始処理をここに記述
    }

    /// <summary>
    /// 更新処理（MonoBehaviourのUpdate相当）
    /// </summary>
    void Update()
    {
        easeInterpolator_.UpdateTime();
        //ターゲットに向く(全方位向く)
        transform.LookAt(targetPlayer.transform);
        //ターゲットへの方向ベクトルの計算(横向きのみ)
        //Vector3 direction = targetPlayer.transform.position - transform.position;
        //direction.y = 0;
        //Quaternion lookRotation = Quaternion.LookRotation(direction,Vector3.up);
        //transform.rotation = lookRotation;
        // 更新処理をここに記述
    }
    private void GenerateBullet()
    {
        easeInterpolator_.Reset();
        if (!IsActive_)
        {
            return;
        }
        GameObject bullet = Instantiate(Sphere);
        bullet.transform.localPosition = transform.position;
        Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.forward * 500.0f);
        Destroy(bullet, 10.0f);

        animator?.SetTrigger(animIDAttack);

    }
    /// <summary>
    /// 移動処理の更新
    /// </summary>
    protected override void UpdateMovement()
    {
        // 移動処理をここに記述
        // 例:
        // splineController_.Move(speed_);
    }

    /// <summary>
    /// 壁との衝突処理
    /// </summary>
    protected override void OnCollideWall()
    {
        // 壁衝突時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// スプライン終端到達時の処理
    /// </summary>
    protected override void OnReachMaxT()
    {
        // スプライン終端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// スプライン始端到達時の処理
    /// </summary>
    protected override void OnReachMinT()
    {
        // スプライン始端到達時の処理をここに記述
        // 例:
        // splineController_.Reverse();
    }

    /// <summary>
    /// プレイヤーに踏みつけられた時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnStompedCore(GameObject player)
    {
        // 踏みつけ時の処理をここに記述
        // 例: 
         OnDamage();
         PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
        
    }
    //public override void OnRequestDestroy()
    //{
    //    Destroy(gameObject);
    //}
    /// <summary>
    /// プレイヤーと横から衝突した時の処理
    /// </summary>
    /// <param name="player">プレイヤーのGameObject</param>
    public override void OnSideHitCore(GameObject player)
    {
        // 横衝突時の処理をここに記述
        // 例:
        // PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
    }
}
