using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
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

    private int dir_;
    [Header("デバッグ用")]
    [SerializeField] private ClearZone clearZone_;
    [SerializeField] string hitName = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Initialize()
    {
        if (thirdPersonController_ == null)
        {
            thirdPersonController_ = GetComponentInChildren<ThirdPersonController>();
        }

        splineController_.splineDirection_ = 1;

       // thirdPersonController_.myEvent = splineController_.CheckUnderSpline;

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

        
       
            
        CheckSplineContainerChange();


        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // 少し上からRayを出すことで地面との誤検出を減らす
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 2.0f;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
            hitName = hit.collider.gameObject.name;

        }
        else
        {
            Debug.DrawRay(rayOrigin, rayDirection * 1000, Color.white);
            hitName = "";
        }
        UpdateCamera();
    }
    private void FixedUpdate()
    {
        
        splineController_.CheckUnderSpline();
    }
    private void InputMovement()
    {
        int inputAxis = 0;
        if (!thirdPersonController_.IsStunned)
        {
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
                OnDamage(0,splineController_.T + 0.5f);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                clearZone_.ClearGame();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                thirdPersonController_.Dying();
                OnTriggerDyingAnim();
            }

        }


        // Spline上のt更新
        splineController_.UpdateT(speed_, inputAxis);
        // アニメーション用の入力設定
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        moveInput.x = inputAxis;
        thirdPersonController_.SetMoveInput(moveInput);
        if (knockbackForce >0)
        {
            knockbackForce += attenuationRate_ * Time.deltaTime;
            splineController_.UpdateT(knockbackForce,dir_);
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
            cameraController_.SetEvaluationInfo(splineController_.EvaluationInfo);
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
            //OnTriggerDyingAnim();
        }
    }

    public override void OnDamage(int damageValue, float enemyT)
    {
        dir_ = -(int)Mathf.Sign(enemyT - splineController_.T);
        OnDamage(damageValue);
        thirdPersonController_.TakeDamage();
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

    public void OnTriggerDyingAnim()
    {
        StartCoroutine(DyingAnim());
    }

    private IEnumerator DyingAnim()
    {
        
        //遷移にかかる時間
        float transitionDuration = 5.0f;
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            //ゲームオーバーの文字を表示

            yield return null;
        }
        TransitionScene.Instance.ToGameOver();
    }
}
