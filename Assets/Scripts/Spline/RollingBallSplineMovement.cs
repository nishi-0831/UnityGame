using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.ProBuilder;
using MySpline;
public class RollingBallSplineMovement : SplineMovementBase
{
    [Header("Rolling Ball Settings")]
    
    [SerializeField]
    private float rollSpeed = 360.0f; // 回転速度
    [SerializeField] private bool bounceOnBounds = false;
    [SerializeField] private float bounceForce = 5.0f;
    
    private Rigidbody rb_;
    private ProBuilderMesh proBuilderMesh_;
    private float radius_;
    public float Radius
    {
        get {  return radius_; } 
        set 
        {
            radius_ = value;
            //if(ballPrefab_ != null)
            transform.localScale = Vector3.one * (radius_ * 2f);
        }
    }
    protected override void Initialize()
    {
        rb_ = GetComponent<Rigidbody>();
        if (rb_ == null)
        {
            rb_ = gameObject.AddComponent<Rigidbody>();
        }

        proBuilderMesh_ = GetComponent<ProBuilderMesh>();
        //splineController_.isMovingLeft = false;
        Debug.Log("Ball:Inititalize");
        IsMovingLeft = false;
    }
   
    public void SetParam(SplineContainer splineContainer,float t,float moveSpeed,float rollSpeed,bool isLeft)
    {
        Debug.Log("Ball:SetParame");
        Debug.Log($"BallT:{t}");
        this.splineController_.currentSplineContainer_ = splineContainer;
        this.splineController_.T = t;
        this.speed_ = moveSpeed;
        this.rollSpeed = rollSpeed;
        //this.splineController_.isMovingLeft=isLeft;
        this.IsMovingLeft = isLeft;
    }
   
    protected override void UpdateMovement()
    {
        splineController_.UpdateT(speed_);
        EvaluationInfo info = splineController_.EvaluationInfo;
        // 基本の移動
        transform.position = info.position + (info.upVector * Radius);
        
        // 転がるアニメーション
        Vector3 tangent = info.tangent;
        
        Vector3 rotationAxis = Vector3.Cross(tangent, Vector3.up);
        float rotationAmount = speed_ * rollSpeed * Time.deltaTime;
        transform.Rotate(rotationAxis, rotationAmount, Space.World);
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
            IsActive_ = false;
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
            IsActive_= false;
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
        
        Debug.Log($"{gameObject.name}: Ball bounced");
    }
    
    /// <summary>
    /// 端に至ったら、そのまま落下
    /// </summary>
    private void Fall()
    {
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        rb_.AddForce(splineController_.EvaluationInfo.tangent * splineMovement.magnitude, ForceMode.VelocityChange);

        Debug.Log($"{gameObject.name}: Ball Fall");
    }
}