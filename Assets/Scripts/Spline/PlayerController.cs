using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent (typeof(SplineController))]
public class PlayerController : SplineMovementBase
{
    
    //[SerializeField] private SplineController splineController_;
    
    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;
    //[SerializeField] GameObject FollowTarget;
    //public float speed_;
    [SerializeField][Range(0f, 30f)] float verticalForce_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        

        splineController_.splineDirection_ = 1;
        
        thirdPersonController_.myEvent = splineController_.CheckUnderSpline;
    }

    // Update is called once per frame
    void Update()
    {
        int inputAxis = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            splineController_.isMovingLeft = true;   
            inputAxis = -1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            splineController_.isMovingLeft = false;
            inputAxis = 1;
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            thirdPersonController_.AddVerticalForce(verticalForce_);
        }
        // アニメーション用の入力設定
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        moveInput.x = inputAxis;
        thirdPersonController_.SetMoveInput(moveInput);
        

        // Spline上のt更新
        splineController_.UpdateT(speed_, inputAxis);

        transform.rotation = splineController_.EvaluationInfo.rotation;

        // ThirdPersonControllerに渡す移動量を計算
        // Splineの移動量 + Y軸の重力/ジャンプ処理
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        Vector3 verticalMovement = new Vector3(0, thirdPersonController_.VerticalVelocity * Time.deltaTime, 0);
        
        Vector3 totalMovement = splineMovement + verticalMovement;
        
        thirdPersonController_.Move(totalMovement);

        if (cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = splineController_.isMovingLeft;
        }
    }
    protected override void OnReachMaxT()
    {
        base.OnReachMaxT();
        splineController_.MoveOtherSplineMinOrMax();
    }

    protected override void OnReachMinT()
    {
        base.OnReachMinT();
        splineController_.MoveOtherSplineMinOrMax();
    }
}
