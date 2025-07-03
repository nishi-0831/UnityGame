using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(SplineController))]
public class PlayerController : SplineMovementBase
{
    //[SerializeFIeld] private int hp_ = 3;
    [SerializeField] float takeDamageInterval_ = 1.0f; // ダメージを受ける間隔
    private bool canTakeDamage_ = true; // ダメージを受けられるかどうか

    [SerializeField] private float knockbackLength_ = 5f; // ノックバックする距離
    [SerializeField] private float knockbackForce = 0;
    //減衰率
    [SerializeField] private float attenuationRate_ = 1f;

    [SerializeField] ThirdPersonController thirdPersonController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer変更検知用
    private SplineContainer previousSplineContainer_;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        splineController_.splineDirection_ = 1;

        thirdPersonController_.myEvent = splineController_.CheckUnderSpline;

        // 初期SplineContainerを記録
        previousSplineContainer_ = splineController_.currentSplineContainer_;
    }

    // Update is called once per frame
    void Update()
    {
       
        
          
        InputMovement();
        transform.rotation = splineController_.EvaluationInfo.rotation;

        // ThirdPersonControllerに渡す移動量を計算
        // Splineの移動量 + Y軸の重力/ジャンプ処理
        Vector3 splineMovement = splineController_.GetSplineMovementDelta();
        Vector3 verticalMovement = new Vector3(0, thirdPersonController_.VerticalVelocity * Time.deltaTime, 0);

        Vector3 totalMovement = splineMovement + verticalMovement;

        thirdPersonController_.Move(totalMovement);

        
        UpdateCamera();
            
        CheckSplineContainerChange();

    }

    private void InputMovement()
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            //thirdPersonController_.AddVerticalForce(verticalForce_);
            thirdPersonController_.TakeDamage();
            OnDamage(0);
        }
        if (!thirdPersonController_.IsStunned)
        {
            // Spline上のt更新
            splineController_.UpdateT(speed_, inputAxis);
        }
        else
        {
            
        }
        // アニメーション用の入力設定
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        moveInput.x = inputAxis;
        thirdPersonController_.SetMoveInput(moveInput);

        if(knockbackForce >0)
        {
            knockbackForce += attenuationRate_ * Time.deltaTime;
            splineController_.UpdateT(knockbackForce);
        }
        else
        {
            knockbackForce = 0;
        }
    }
    protected override void UpdateMovement()
    {
        // Splineの移動はUpdateメソッドで行うため、ここでは何もしない
    }

    private void UpdateCamera()
    {
        if (cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = splineController_.isMovingLeft;
        }
    }

    /// <summary>
    /// SplineContainer変更をチェックし、カメラに通知
    /// </summary>
    private void CheckSplineContainerChange()
    {
        if (splineController_.currentSplineContainer_ != previousSplineContainer_)
        {
            Debug.Log("SplineContainer changed!");

            // カメラにSplineContainer変更を通知
            if (cameraController_ != null)
            {
                // 新しいSplineのベース高度を計算（現在のプレイヤー位置のY座標を使用）
                float newBaseY = splineController_.EvaluationInfo.position.y;
                cameraController_.OnSplineContainerChanged(newBaseY);
            }

            previousSplineContainer_ = splineController_.currentSplineContainer_;
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

    public override void OnDamage()
    {
        base.OnDamage();
    }

    public override void OnDamage(int damageValue)
    {
        if (!canTakeDamage_)
        {
            return; // ダメージを受けられない場合は何もしない
        }
        hp_ -= damageValue;
        knockbackForce = Mathf.Sqrt(splineController_.GetSplineMovementT(knockbackLength_) * -2f * attenuationRate_);
        StartCoroutine(WaitCanTakeDamage());
        if (hp_ <= 0)
        {
            //TransitionScene.Instance.ToGameOver();
        }
    }

    private IEnumerator WaitCanTakeDamage()
    {
        if (canTakeDamage_)
        {
            canTakeDamage_ = false;
            yield return new WaitForSeconds(takeDamageInterval_);
            canTakeDamage_ = true;
            //knockbackForce = 0;
        }
    }
}
