using UnityEngine;

public class RollingBallSplineMovement : SplineMovementBase
{
    [Header("Rolling Ball Settings")]
    
    [SerializeField]
    private float rollSpeed = 360.0f; // 回転速度
    [SerializeField] private bool bounceOnBounds = false;
    [SerializeField] private float bounceForce = 5.0f;
    
    private Rigidbody rb_;

    
    protected override void Start()
    {
        base.Start();
        rb_ = GetComponent<Rigidbody>();
        if (rb_ == null)
        {
            rb_ = gameObject.AddComponent<Rigidbody>();
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        //splineController_.isMovingLeft = false;
        IsMovingLeft = false;
    }
    public void SetParam(float t,float moveSpeed,float rollSpeed,bool isLeft)
    {
        this.splineController_.t_ = t;
        this.speed_ = moveSpeed;
        this.rollSpeed = rollSpeed;
        //this.splineController_.isMovingLeft=isLeft;
        this.IsMovingLeft = isLeft;
    }
    //protected override void Update()
    //{
    //    base .Update();
    //}
    protected override void UpdateMovement()
    {
        base.UpdateMovement();
        // 基本の移動
        splineController_.Move(speed_);

        // 転がるアニメーション
        Vector3 tangent = splineController_.GetSplineTangent();
        Vector3 rotationAxis = Vector3.Cross(tangent, Vector3.up);
        float rotationAmount = speed_ * rollSpeed * Time.deltaTime;
        transform.Rotate(rotationAxis, rotationAmount, Space.World);
    }
    
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();

        if (bounceOnBounds)
        {
            HandleBounce();
        }
        else
        {
            IsActive_ = false;
        }
    }
    
    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        if (bounceOnBounds)
        {
            HandleBounce();
        }
        else
        {
            IsActive_= false;
        }
    }
    
    private void HandleBounce()
    {
        splineController_.Reverse();
        
        // 物理的な跳ね返り効果
        if (rb_ != null)
        {
            Vector3 bounceDirection = Vector3.up + splineController_.GetSplineTangent() * 0.5f;
            rb_.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }
        
        Debug.Log($"{gameObject.name}: Ball bounced");
    }
    
    /// <summary>
    /// 端に至ったら、そのまま落下
    /// </summary>
    private void Fall()
    {

    }
}