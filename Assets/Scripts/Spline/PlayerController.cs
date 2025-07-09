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
    [SerializeField]private LayerMask groundLayer_;
    private int dir_;
    [Header("デバッグ用")]
    [SerializeField] private ClearZone clearZone_;
    [SerializeField] private MySpline.EvaluationInfo evaluationInfo_;
    public Vector3 halsExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;
    [SerializeField] private Rigidbody rb_;
    [SerializeField] private CapsuleCollider capsuleCollider_;

    // Splineの垂直方向の変化とジャンプを統合するための変数
    private Vector3 previousSplinePosition_;
    private bool isFirstFrame_ = true;

    protected override void Initialize()
    {
        //groundLayer_ = layerSettings_.groundLayer;

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
        evaluationInfo_ = splineController_.EvaluationInfo;
        InputMovement();
        transform.rotation = splineController_.EvaluationInfo.rotation;

        // Splineの基準位置を取得
        Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();
        
        // Splineの垂直方向の変化量を計算
        Vector3 splineVerticalDelta = Vector3.zero;
        if (!isFirstFrame_)
        {
            Vector3 splineDelta = currentSplinePosition - previousSplinePosition_;
            splineVerticalDelta = new Vector3(0, splineDelta.y, 0); // Y成分のみ取得
        }
        
        // ジャンプによる垂直方向の移動量を計算
        Vector3 jumpVerticalMovement = Vector3.up * animController_.VerticalVelocity * Time.deltaTime;
        vertical = jumpVerticalMovement; // デバッグ用

        // 現在のプレイヤー位置から水平方向の成分を取得
        Vector3 currentHorizontalPosition = new Vector3(currentSplinePosition.x, transform.position.y, currentSplinePosition.z);
        
        // 新しい位置 = Splineの水平位置 + Splineの垂直変化 + ジャンプの垂直移動
        Vector3 newPosition = currentHorizontalPosition + splineVerticalDelta + jumpVerticalMovement;
        //transform.position = newPosition;
        rb_.MovePosition(newPosition);
        
        // 前フレームの位置を更新
        previousSplinePosition_ = currentSplinePosition;
        isFirstFrame_ = false;
        
        CheckSplineContainerChange();
        UpdateCamera();
    }

    public void CheckSpline(SplineContainer splineContainer)
    {
        if (splineContainer == null)
        {
            Debug.Log("checksplineContainer==null");
            return;
        }
        if(splineController_.currentSplineContainer_ != splineContainer)
        {
            splineController_.ChangeOtherSpline(splineContainer);
        }
    }

   
    private void OnCollisionEnter(Collision collision)
    {
        GameObject groundObj = collision.gameObject;
        Debug.Log("OnCollisionEnter");
        if (groundObj.layer == (int)Mathf.Log(splineController_.splineLayerSettings_.groundLayer, 2))
        {
            SplineContainer collisionContainer = groundObj.GetComponent<SplineContainer>();
            CheckSpline(collisionContainer);
            animController_.Grounded = true;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        GameObject groundObj = collision.gameObject;
        Debug.Log("OnCollisionStay");

        if (groundObj.layer == (int)Mathf.Log(splineController_.splineLayerSettings_.groundLayer, 2))
        {
            animController_.Grounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        animController_.Grounded = false;
    }
    private void InputMovement()
    {
        
        if (!animController_.IsStunned)
        {
            if(inputs_.move.x == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if(inputs_.move.x == 1)
            {
                splineController_.isMovingLeft = false;
            }

            
            if (Input.GetKeyDown(KeyCode.T))
            {
                animController_.TakeDamage();
                OnDamage(0,splineController_.T + 0.5f);
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                animController_.AddVerticalForce(verticalForce_);
            }
        }

        // Spline上のt更新
        splineController_.UpdateT(speed_, (int)inputs_.move.x);
        // アニメーション用の入力設定
        UnityEngine.Vector2 moveInput = UnityEngine.Vector2.zero;
        //moveInput.x = ;
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
        base.OnReachMaxT();
        //splineController_.MoveOtherSplineMinOrMax();
    }

    protected override void OnReachMinT()
    {
        base.OnReachMinT();
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

    
}
