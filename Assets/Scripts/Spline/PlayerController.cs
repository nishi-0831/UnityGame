using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using MySpline;
using StarterAssets;

[RequireComponent(typeof(SplineController))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : SplineMovementBase
{
    [SerializeField] float takeDamageInterval_ = 1.0f; // ダメージを受ける間隔
    private bool canTakeDamage_ = true; // ダメージを受けられるかどうか

    [SerializeField] private float knockbackLength_ = 5f; // ノックバックする距離
    [SerializeField] private float knockbackForce = 0;
    //減衰率
    [SerializeField] private float attenuationRate_ = 1f;

    [SerializeField] AnimationController animController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer変更検知用
    private SplineContainer previousSplineContainer_;
    [SerializeField] private LayerMask groundLayer_;
    private int dir_;
    [Header("デバッグ用")]
    [SerializeField] private ClearZone clearZone_;
    [SerializeField] private MySpline.EvaluationInfo evaluationInfo_;
    public Vector3 center_;
    public Vector3 halfExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;
    [SerializeField] private Rigidbody rb_;
    [SerializeField] private CapsuleCollider capsuleCollider_;

    // Splineの垂直方向の変化とジャンプを統合するための変数
    private Vector3 previousSplinePosition_;
    private Vector3 previousOffSplinePosition_;
    private bool isFirstFrame_ = true;

    // Spline範囲外での移動制御
    [SerializeField] private bool isOffSpline_ = false; // Spline範囲外にいるかどうか
    [SerializeField] private Vector3 offSplineVelocity_; // Spline範囲外での移動速度
    [SerializeField] private Vector3 lastValidTangent_; // 最後の有効なタンジェント
    private float lastValidDir = 0f;

    protected override void Initialize()
    {
        if (animController_ == null)
        {
            animController_ = GetComponent<AnimationController>();
        }

        splineController_.splineDirection_ = 1;

        // 初期SplineContainerを記録
        previousSplineContainer_ = splineController_.currentSplineContainer_;

        // 初期Spline位置を記録
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
    }

    void Update()
    {
#if true
        evaluationInfo_ = splineController_.EvaluationInfo;
        InputMovement();
        

        HandleSplineMovement();

        // 地面判定をAnimationControllerに反映
        animController_.Grounded = Physics.CheckBox(transform.position + center_, halfExtends_, transform.rotation, groundLayer_);

        // 前フレームの位置を更新
        if (!isOffSpline_)
        {
            previousSplinePosition_ = splineController_.GetSplineMeshPos();
        }
        else
        {
            previousOffSplinePosition_ = transform.position;
        }
            isFirstFrame_ = false;

        CheckSplineContainerChange();
        UpdateCamera();

        Debug.DrawRay(transform.position, offSplineVelocity_ * 1000f);
#endif
        //InputMovement();
        // 空中にいる場合は下方向のSplineをチェック
        if (!animController_.Grounded)
        {
            splineController_.CheckUnderSpline();
        }
    }

    private void HandleSplineMovement()
    {
        float currentT = splineController_.T;

        // Spline範囲内の場合
        if (currentT >= 0f && currentT <= 1f && !isOffSpline_)
        {
            // 通常のSpline移動
            transform.rotation = splineController_.EvaluationInfo.rotation;

            Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();

            // Splineの垂直方向の変化量を計算
            Vector3 splineVerticalDelta = Vector3.zero;
            if (!isFirstFrame_)
            {
                Vector3 splineDelta = currentSplinePosition - previousSplinePosition_;
                splineVerticalDelta = new Vector3(0, splineDelta.y, 0);
            }

            // ジャンプによる垂直方向の移動量を計算
            Vector3 jumpVerticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;
            

            // 現在のプレイヤー位置から水平方向の成分を取得
            Vector3 currentHorizontalPosition = new Vector3(currentSplinePosition.x, transform.position.y, currentSplinePosition.z);

            // 新しい位置 = Splineの水平位置 + Splineの垂直変化 + ジャンプの垂直移動
            Vector3 newPosition = currentHorizontalPosition + splineVerticalDelta + jumpVerticalMovement;
            transform.position = newPosition;
            //rb_.MovePosition(newPosition);
        }
        // Spline範囲外の場合
        else
        {
            //isOffSpline_ = true;
            HandleOffSplineMovement(currentT);
        }
    }

    private void HandleOffSplineMovement(float currentT)
    {
        Vector3 offSplineVerticalDelta = Vector3.zero;
        if (!isOffSpline_)
        {
            
            isOffSpline_ = true;
            // 最後の有効なタンジェントを保存
            if (currentT > 1f)
            {
                //lastValidTangent_ = splineController_.GetEvaluationInfo(1f).tangent;
                lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 1f);
                lastValidDir = 1f;
            }
            else if (currentT < 0f)
            {
                lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 0);
                lastValidDir = -1f;
            }
            
            if (!isFirstFrame_)
            {
                Vector3 offSplineDelta = transform.position - previousOffSplinePosition_;
                offSplineVerticalDelta = new Vector3(0, offSplineDelta.y, 0);
            }
            // 初期速度を設定（水平方向はSplineのタンジェント、垂直方向は現在の垂直速度）
            offSplineVelocity_ = new Vector3(transform.forward.x, 0, transform.forward.z);
            
            offSplineVelocity_ = offSplineVelocity_.normalized * (speed_);

            Debug.Log($"Off Spline! Last tangent");
            

        }
        // Spline範囲外での移動
        Vector3 horizontalMovement = new Vector3(offSplineVelocity_.x, 0, offSplineVelocity_.z);
        
        Vector3 verticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;

        transform.position += horizontalMovement + verticalMovement;
        // 回転をタンジェント方向に設定
        if (lastValidTangent_ != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lastValidDir * lastValidTangent_, Vector3.up);
        }

       
    }

    // SplineControllerから呼ばれる落下時の新しいSpline発見処理
    public void OnFoundNewSpline(SplineContainer newSplineContainer)
    {
#if true
        if ( newSplineContainer != null)
        {
            Debug.Log($"Found new spline: {newSplineContainer.name}");
            isOffSpline_ = false;

            // 新しいSplineに移行
            splineController_.ChangeOtherSpline(newSplineContainer);

            // 位置を新しいSplineに合わせて調整
            Vector3 newSplinePosition = splineController_.GetSplineMeshPos();
            transform.position = new Vector3(newSplinePosition.x, transform.position.y, newSplinePosition.z);

            previousSplinePosition_ = newSplinePosition;
        }
#endif
        //splineController_.ChangeOtherSpline(newSplineContainer);
        //isOffSpline_ = false;
    }

    public void CheckSpline(SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.Log("checksplineContainer==null");
            return;
        }
        if (splineController_.currentSplineContainer_ != splineContainer)
        {
            splineController_.ChangeOtherSpline(splineContainer);
        }
    }

   

    private void InputMovement()
    {
        if (!animController_.IsStunned)
        {
            if (inputs_.move.x == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if (inputs_.move.x == 1)
            {
                splineController_.isMovingLeft = false;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                animController_.TakeDamage();
                OnDamage(0, splineController_.T + 0.5f);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                clearZone_.ClearGame();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                animController_.Dying();
                OnTriggerDyingAnim();
            }
            if (inputs_.jump && animController_.Grounded)
            {
                animController_.AddVerticalForce(verticalForce_);
                //rb_.AddForce(new Vector3(0, verticalForce_, 0), ForceMode.Force);
            }
        }

        // Spline上のt更新（範囲外でも更新を続ける）
        splineController_.UpdateT(speed_, (int)inputs_.move.x);

        // アニメーション用の入力設定
        animController_.SetMoveInput(inputs_.move);

        if (knockbackForce > 0)
        {
            knockbackForce += attenuationRate_ * Time.deltaTime;
            splineController_.UpdateT(knockbackForce, dir_);
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
        // Spline範囲外移動に移行するため、基底クラスの処理はスキップ
        Debug.Log($"{gameObject.name}: Reached Max T - transitioning to off-spline movement");
        //splineController_.MoveOtherSplineMinOrMax();

    }

    protected override void OnReachMinT()
    {
        // Spline範囲外移動に移行するため、基底クラスの処理はスキップ
        Debug.Log($"{gameObject.name}: Reached Min T - transitioning to off-spline movement");
        //splineController_.MoveOtherSplineMinOrMax();
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
        animController_.TakeDamage();
    }

    private IEnumerator WaitCanTakeDamage()
    {
        if (canTakeDamage_)
        {
            canTakeDamage_ = false;
            yield return new WaitForSeconds(takeDamageInterval_);
            canTakeDamage_ = true;
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

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (animController_ != null && animController_.Grounded)
            Gizmos.color = transparentGreen;
        else
            Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawCube(transform.position + center_, halfExtends_);
    }
}
