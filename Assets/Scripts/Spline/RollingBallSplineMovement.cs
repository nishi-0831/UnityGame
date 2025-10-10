using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.ProBuilder;
using MySpline;

public class RollingBallSplineMovement : PlayerInteractableBase
{
    [Header("Rolling Ball Settings")]
    [Space(16)]
    [Header("自動で曲線から落ちる時間(秒)")]
    [SerializeField] private float lifespan_ = 5.0f;
    [Header("落下してから破棄されるまでの時間")]
    [SerializeField]private float destroyDelay_ = 3.0f;

    [SerializeField] private float rollSpeed = 360.0f;
    [SerializeField] private bool bounceOnBounds = false;
    [SerializeField] private float bounceForce = 5.0f;
    //[SerializeField] private float knockbackForce = 10.0f;
    [Header("触れたら曲線から落ちるトリガーのレイヤー")]
    [SerializeField] private LayerMask destroyTriggerLayer_;
    private Rigidbody rb_;
    private Vector3 lastVelosity_;
    //生成された時間
    private float instantiatedTime_ = 0;
    [SerializeField] private float radius_;

    public float Radius
    {
        get { return radius_; }
        set
        {
            radius_ = value;
            transform.localScale = Vector3.one * (radius_ * 2f);
        }
    }

    protected override void Initialize()
    {
        instantiatedTime_ = Time.time;
        rb_ = GetComponent<Rigidbody>();
        if (rb_ == null)
        {
            rb_ = gameObject.AddComponent<Rigidbody>();
        }
        radius_ = transform.localScale.x / 2f;
    }
    protected override void Start()
    {
        base.Start();
        FollowTarget.transform.rotation = splineController_.EvaluationInfo.rotation;
    }
    public void SetParam(SplineContainer splineContainer, float t, float moveSpeed, float rollSpeed, bool isLeft,float lifeSpan)
    {
        this.splineController_.currentSplineContainer_ = splineContainer;
        splineController_.SetSplineMeshRadius();

        this.splineController_.Progress = t;
        this.speed_ = moveSpeed;
        this.rollSpeed = rollSpeed;
        this.IsMovingLeft = isLeft;
        this.lifespan_ = lifeSpan;
    }

    protected override void UpdateMovement()
    {
        splineController_.UpdateProgress(speed_);
        EvaluationInfo info = splineController_.EvaluationInfo;
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        lastVelosity_ = splineMovement / Time.deltaTime;

        // 基本の移動
        transform.position = info.position + (info.upVector * (Radius + splineController_.SplineMeshRadius / 2.0f) );

        // 転がるアニメーション
        Vector3 tangent = info.tangent;
        Vector3 rotationAxis = Vector3.Cross(tangent, info.upVector);
        float rotationAmount = rollSpeed * Time.deltaTime;

        transform.Rotate(rotationAxis, -rotationAmount, Space.World);

        if (Time.time - instantiatedTime_ > lifespan_)
        {
            Disable();
            Fall();
        }
    }

    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        CancelOnReachMaxT();
        if (bounceOnBounds)
        {
            HandleBounce();
        }
        else
        {
            Disable();
            Fall();
        }
    }

    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        CancelOnReachMinT();
        if (bounceOnBounds)
        {
            HandleBounce();
        }
        else
        {
            Disable();
            Fall();
        }
    }

    private void HandleBounce()
    {
        splineController_.Reverse();

        // 物理的な跳ね返り効果
        if (rb_ != null)
        {
            Vector3 bounceDirection = Vector3.up + splineController_.EvaluationInfo.tangent * 0.5f;
            rb_.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 端に至ったら、そのまま落下
    /// </summary>
    private void Fall()
    {
        rb_.linearVelocity = lastVelosity_.magnitude * splineController_.EvaluationInfo.tangent.normalized;
        rb_.useGravity = true;
        Destroy(gameObject, destroyDelay_);
    }

    // IPlayerInteractable実装
    public override void OnStompedCore(GameObject player)
    {
        PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);

        // ボールを破壊
        Disable();
        Destroy(gameObject,0.1f);
    }

    public override void OnSideHitCore(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player, DamageToPlayer);
        PlayerInteractionUtils.ApplySideBounce(player, Progress);
    }
    public void OnTriggerEnter(Collider other)
    {
        var hitLayerMask = (int)Mathf.Log(destroyTriggerLayer_.value, 2);
         
        if ((hitLayerMask == other.gameObject.layer))
        {
            Disable();
            Fall();
        }
    }
}