using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.ProBuilder;
using MySpline;

public class RollingBallSplineMovement : SplineMovementBase, IPlayerInteractable
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

    [SerializeField] private bool canBeStomped = true;
    [SerializeField] private int damageToPlayer = 1;

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
        //rb_.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        //Debug.Log("Ball:Initialize");
        IsMovingLeft = false;

        radius_ = transform.localScale.x / 2f;
        FollowTarget.transform.rotation = splineController_.EvaluationInfo.rotation;
    }

    public void SetParam(SplineContainer splineContainer, float t, float moveSpeed, float rollSpeed, bool isLeft,float lifeSpan)
    {
        //Debug.Log("Ball:SetParam");
        //Debug.Log($"BallT:{t}");
        this.splineController_.currentSplineContainer_ = splineContainer;
        this.splineController_.T = t;
        this.speed_ = moveSpeed;
        this.rollSpeed = rollSpeed;
        this.IsMovingLeft = isLeft;
        this.lifespan_ = lifeSpan;
    }

    protected override void UpdateMovement()
    {

        splineController_.UpdateT(speed_);
        EvaluationInfo info = splineController_.EvaluationInfo;
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        lastVelosity_ = splineMovement / Time.deltaTime;

        // 基本の移動
        transform.position = info.position + (info.upVector * Radius);

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
    public bool OnStompedByPlayer(GameObject player)
    {
        if (!IsActive_ && !canBeStomped)
            return false;

        //Debug.Log($"{gameObject.name} was stomped by player - Ball destroyed!");

        // プレイヤーに大きな跳ね返りを与える
        var playerThirdPerson = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (playerThirdPerson != null)
        {
            playerThirdPerson.AddVerticalForce(8f); // 高くジャンプ
        }

        // ボールを破壊
        Disable();
        Destroy(gameObject,0.1f);

        return true; // 踏みつけ成功
    }

    public void OnSideCollisionWithPlayer(GameObject player)
    {
        if (!IsActive_)
            return;

        //Debug.Log($"{gameObject.name} damaged player!");

        // プレイヤーにダメージを与える処理
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // ダメージ処理をここに実装
            playerController.OnDamage(damageToPlayer, splineController_.T);
            //Debug.Log($"Player took {damageToPlayer} damage!");
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        var hitLayerMask = (int)Mathf.Log(destroyTriggerLayer_.value, 2);
         
        if ((hitLayerMask == other.gameObject.layer))
        {
            //Debug.Log("OnTrigger");
            Disable();
            Fall();
        }
        else
        {
            //Debug.Log(hitLayerMask);
            //Debug.Log("OnNotTrigger");
        }
    }
}