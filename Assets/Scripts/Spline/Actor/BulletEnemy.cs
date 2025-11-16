using UnityEngine;
[RequireComponent(typeof(EaseInterpolator))]

/// <summary>
/// BulletEnemyの説明をここに記述
/// </summary>
public class BulletEnemy : PlayerInteractableBase
{
    public GameObject Sphere;
    public GameObject targetPlayer;
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private float attackInterval_ = 5.0f;
    [SerializeField] private float bulletSpeed_ = 30.0f;
    [SerializeField] private float battleDistance_ = 50.0f;
    [SerializeField] private EaseInterpolator easeInterpolator_;
    [SerializeField] private Vector3 bulletOffset_ = Vector3.zero;
    [HideInInspector] protected int animIDBattle;
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

        animIDBattle = Animator.StringToHash("Battle");
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
        Vector3 dir = Vector3.Normalize(targetPlayer.transform.position - transform.position);
        float distanceSqr = Vector3.SqrMagnitude(transform.position - targetPlayer.transform.position);
        if (distanceSqr < Mathf.Pow(battleDistance_,2))
        {
            animator.SetBool(animIDBattle, true);
        }
        else
        {
            animator.SetBool(animIDBattle, false);
            easeInterpolator_.Reset();
        }


        if (animator.GetBool(animIDBattle))
        {
            //ターゲットに向く(全方位向く)
            transform.position = splineController_.GetEvaluationInfo(Progress).position;
            Vector3 toTarget = targetPlayer.transform.position - transform.position;
            if (toTarget.sqrMagnitude > 1e-6f)
            {
                Vector3 desiredUp = toTarget.normalized;
                // rotationRootの現状のupをdesiredUpに合わせる
                rotationRoot.rotation = Quaternion.FromToRotation(-rotationRoot.up, desiredUp) * rotationRoot.rotation;
            }
            easeInterpolator_.UpdateTime();
        }

        Debug.DrawLine(transform.position, transform.position + dir * battleDistance_);
    }
    private void GenerateBullet()
    {
        easeInterpolator_.Reset();
        if (!IsActive_)
        {
            return;
        }
        //GameObject bullet = Instantiate(Sphere);

        Vector3 origin = transform.position + bulletOffset_;
        Vector3 toTarget = targetPlayer.transform.position - transform.position;
        Vector3 baseDir = toTarget.sqrMagnitude > 1e-6f ? toTarget.normalized : transform.forward;

        Vector3 up = rotationRoot != null ? rotationRoot.up : Vector3.up;
        CreateBullet(origin, baseDir);

        Vector3 leftDir = Quaternion.AngleAxis(-spreadAngle, Vector3.up) * baseDir;
        Vector3 rightDir = Quaternion.AngleAxis(spreadAngle, Vector3.up) * baseDir;
        CreateBullet(origin, leftDir);
        CreateBullet(origin, rightDir);

        animator?.SetTrigger(animIDAttack);
    }
    private void CreateBullet(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(Sphere);
        bullet.transform.position = position;
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * bulletSpeed_, ForceMode.Impulse);
        }
        Destroy(bullet, 10.0f);
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
        animator.SetBool(animIDBattle, false);
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
