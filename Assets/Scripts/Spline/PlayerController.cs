using MySpline;
using StarterAssets;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineController))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : SplineMovementBase
{
    [SerializeField] private int hp_ = 3;
    [SerializeField] float takeDamageInterval_ = 1.0f; // ダメージを受ける間隔
    [SerializeField] private bool canTakeDamage_ = true; // ダメージを受けられるかどうか

    [SerializeField] private float knockbackLength_ = 5f; // ノックバックする距離
    [SerializeField] private float knockbackForce = 0;
    //減衰率
    [SerializeField] private float attenuationDelta_ = 1f;

    [SerializeField] AnimationController animController_;
    [SerializeField] CameraController cameraController_;

    [SerializeField][Range(0f, 30f)] float verticalForce_;

    // SplineContainer変更検知用
    private SplineContainer previousSplineContainer_;
    [SerializeField] private LayerMask groundLayer_;
    private int knockbackDir_;
    [Header("デバッグ用")]
    [SerializeField] private ClearZone clearZone_;
    public Vector3 center_;
    public Vector3 halfExtends_;
    public Vector3 vertical;
    [SerializeField] private StarterAssetsInputs inputs_;

    // LaneChangeZone用のSpline変更フラグ
    [SerializeField] private bool hasSplineChangedThisFrame_ = false;
    [SerializeField] private int splineChangeFrameCount_ = 0; // 複数フレーム保護用

    // Splineの垂直方向の変化とジャンプを統合するための変数
    [SerializeField]private Vector3 previousSplinePosition_;
    private Vector3 previousOffSplinePosition_;
    private bool isFirstFrame_ = true;

    // Spline範囲外での移動制御
    [SerializeField] private bool isOffSpline_ = false; // Spline範囲外にいるかどうか
    [SerializeField] private Vector3 offSplineVelocity_; // Spline範囲外での移動速度
    [SerializeField] private Vector3 lastValidTangent_; // 最後の有効なタンジェント
    private float lastValidDir = 0f;
    private bool isSmashed = false;
    [SerializeField] private SmashPlayer smashPlayer_;
    [SerializeField] private Rigidbody rb_;
    // スマッシュ状態管理
    [SerializeField] private bool isBeingSmashed_ = false;
    [SerializeField] private GameObject respawnPoint_;
    [SerializeField]
    Vector3 actualMovement;
    [SerializeField]Vector3 desiredMovement = Vector3.zero;
    Vector3 splineVerticalDelta = Vector3.zero;
    [SerializeField] Vector3 splineDelta;
    [SerializeField] private CharacterController characterController_;
    [SerializeField]Vector3 jumpVerticalMovement;
    [SerializeField] Vector3 newPosition;
    [SerializeField] Vector3 actualNoVerticalMovementPos;
    [SerializeField]Vector3 inputMovement;
    [SerializeField]Vector3 currentHorizontalPosition;
    [SerializeField]Vector3 knockbackMovement = Vector3.zero;
    public JumpControllerVariableHeight jumpControllerVariableHeight_;
    public float T { get { return splineController_.Progress; } }
    protected override void Initialize()
    {
        if (animController_ == null)
        {
            animController_ = GetComponent<AnimationController>();
        }
        characterController_ = GetComponent<CharacterController>();
        splineController_.splineDirection_ = 1;

        // 初期SplineContainerを記録
        previousSplineContainer_ = splineController_.currentSplineContainer_;

        // 初期Spline位置を記録
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
        inputs_.onReleaseJumpBtn += jumpControllerVariableHeight_.Release;
    }

    

    private void HandleSplineMovement()
    {
        float currentT = splineController_.Progress;
        desiredMovement = Vector3.zero;
        actualMovement = Vector3.zero;
        
        // Spline範囲内の場合
        if (currentT >= 0f && currentT <= 1f && !isOffSpline_)
        {

            // 通常のSpline移動
            transform.rotation = splineController_.EvaluationInfo.rotation;

            //Vector3 currentSplinePosition = splineController_.GetSplineMeshPos();

            //// Splineの垂直方向の変化量を計算（前フレームの位置と比較）
            //if (!isFirstFrame_)
            //{
            //    splineDelta = currentSplinePosition - previousSplinePosition_;
            //    splineVerticalDelta = new Vector3(0, splineDelta.y, 0);
            //}
            //else
            //{
            //    splineVerticalDelta = Vector3.zero;
            //    splineDelta = Vector3.zero;
            //}

            //// ジャンプによる垂直方向の移動量を
            //jumpVerticalMovement = Vector3.up * animController_.CurrentJumpOffsetY;

            //// 入力による水平移動量を計算



            //// ノックバックによる移動量を計算
            //if (knockbackForce > 0)
            //{
            //    Vector3 knockbackDirection = splineController_.EvaluationInfo.tangent.normalized;
            //    if (splineController_.isMovingLeft)
            //    {
            //        knockbackDirection *= -1;
            //    }
            //    knockbackDirection *= knockbackDir_;
            //    knockbackMovement = knockbackDirection * knockbackForce * Time.deltaTime;
            //}
            //else
            //{
            //    knockbackMovement = Vector3.zero;
            //}

            //// 入力による移動量を計算（Splineに沿った水平移動のみ）
            //inputMovement = Vector3.zero;
            //if (inputDir != 0)
            //{
            //    Vector3 inputDirection = splineController_.EvaluationInfo.tangent.normalized;
            //    if (splineController_.isMovingLeft)
            //    {
            //        inputDirection *= -1;
            //    }
            //    inputDirection *= inputDir * splineController_.splineDirection_;
            //    inputMovement = inputDirection * speed_ * Time.deltaTime;
            //}

            //// 実際の移動計算方法を変更
            //// 現在位置をベースに、各移動成分を加算
            //Vector3 horizontalSplineMovement = new Vector3(splineDelta.x, 0, splineDelta.z);

            //newPosition = transform.position + 
            //             splineVerticalDelta + 
            //             jumpVerticalMovement + 
            //             inputMovement + 
            //             knockbackMovement + 
            //             horizontalSplineMovement;

            //desiredMovement = newPosition - transform.position;

            //// 非常に小さい移動量は無視
            //if (desiredMovement.magnitude <= 0.001f)
            //{
            //    desiredMovement = Vector3.zero;
            //}

            //// CharacterControllerで移動
            //Vector3 startPos = transform.position;
            //if (desiredMovement != Vector3.zero)
            //{
            //    characterController_.Move(desiredMovement);
            //}
            //actualMovement = transform.position - jumpVerticalMovement - startPos;

            if (!animController_.IsStunned && !isBeingSmashed_)
            {
                if (inputs_.move.x != 0)
                {
                    splineController_.Move(speed_);
                    
                }
            }
            // Splineに沿った移動のみでt値更新（垂直移動を除外）
            //Vector3 horizontalActualMovement = new Vector3(actualMovement.x, 0, actualMovement.z);
            //splineController_.UpdateProgressFromMovement(actualMovement);
            jumpControllerVariableHeight_.Tick(splineController_.EvaluationInfo.position);

        }
        // Spline範囲外の場合
        else
        {

            desiredMovement = HandleOffSplineMovement(currentT);
            
            Vector3 startPos = transform.position;
            if(desiredMovement != Vector3.zero)
            {
                characterController_.Move(desiredMovement);
            }
            actualMovement = transform.position - startPos;
            
            // オフSpline時は実際の移動量でt値更新
            splineController_.UpdateProgressFromMovement(actualMovement);
        }
    }

    private Vector3 HandleOffSplineMovement(float currentT)
    {
        Vector3 offSplineVerticalDelta = Vector3.zero;
        if (!isOffSpline_)
        {

            isOffSpline_ = true;
            // 最後の有効なタンジェントを保存
            if (currentT > 1f)
            {
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

            offSplineVelocity_ = offSplineVelocity_.normalized * speed_ * Time.deltaTime;

            Debug.Log($"Off Spline! Last tangent");


        }
        // Spline範囲外での移動
        Vector3 horizontalMovement = new Vector3(offSplineVelocity_.x, 0, offSplineVelocity_.z);
        
        Vector3 verticalMovement = Vector3.up * animController_.CurrentJumpOffsetY;

        // 回転をタンジェント方向に設定
        if (lastValidTangent_ != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lastValidDir * lastValidTangent_, Vector3.up);
        }

        return horizontalMovement + verticalMovement;
    }

    protected override void OnReachMaxT()
    {
        //lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 1f);
        //lastValidDir = 1f;
        //CalculateOffSplineVelocity_();
    }

    protected override void OnReachMinT()
    {
        //lastValidTangent_ = SplineUtility.EvaluateTangent<Spline>(splineController_.currentSplineContainer_.Spline, 0);
        //lastValidDir = -1f;
        //CalculateOffSplineVelocity_();
    }

    private void CalculateOffSplineVelocity_()
    {
        
        // 初期速度を設定（水平方向はSplineのタンジェント、垂直方向は現在の垂直速度）
        offSplineVelocity_ = new Vector3(transform.forward.x, 0, transform.forward.z);

        offSplineVelocity_ = offSplineVelocity_.normalized * speed_ * Time.deltaTime;

        Debug.Log($"Off Spline! Last tangent");
    }
    // SplineControllerから呼ばれる落下時の新しいSpline発見処理
    public void OnFoundNewSpline(SplineContainer newSplineContainer)
    {
        // LaneChangeZoneによる強制変更の場合はスキップ
        if (hasSplineChangedThisFrame_)
        {
            Debug.Log($"[OnFoundNewSpline] Skipping due to forced change this frame. Found: {newSplineContainer?.name}");
            return;
        }

#if true
        if ( newSplineContainer != null)
        {
            Debug.Log($"[OnFoundNewSpline] Processing new spline: {newSplineContainer.name}");
            isOffSpline_ = false;

            // 新しいSplineに移行
            splineController_.ChangeOtherSpline(newSplineContainer);

            //新しいSplineMeshのRadiusを取得
            splineController_.SetSplineMeshRadius();
            // 位置を新しいSplineに合わせて調整
            Vector3 newSplinePosition = splineController_.GetSplineMeshPos();
            transform.position = new Vector3(newSplinePosition.x, transform.position.y, newSplinePosition.z);

            previousSplinePosition_ = newSplinePosition;
            Debug.Log($"[OnFoundNewSpline] Completed spline change to: {newSplineContainer.name}");
        }
#endif
    }
    private void InputMovement()
    {
        // スマッシュ中は入力を完全に無効化
        if (isBeingSmashed_)
        {
            return;
        }

        int dir = 0;
        if (!animController_.IsStunned)
        {
            if (inputs_.move.x != 0)
            {
                dir = (int)Mathf.Sign(inputs_.move.x);
            }

            if (dir == -1)
            {
                splineController_.isMovingLeft = true;
            }
            else if (dir == 1)
            {
                splineController_.isMovingLeft = false;
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
                jumpControllerVariableHeight_.StartJump(splineController_.EvaluationInfo.position.y);
            }
        }

        // アニメーション用の入力設定
        animController_.SetMoveInput(inputs_.move);

        // ノックバック処理の減衰
        if (knockbackForce > 0)
        {
            knockbackForce -= attenuationDelta_ * Time.deltaTime;
            if (knockbackForce < 0)
            {
                knockbackForce = 0;
            }
        }
    }

    protected override void UpdateMovement()
    {
        // スマッシュ中はカメラ更新のみ停止、他の処理は継続
        if (isBeingSmashed_)
        {
            // スマッシュ中は移動処理をスキップ
            return;
        }

        // LaneChangeZoneによる強制変更が発生した場合は数フレーム保護
        if (splineChangeFrameCount_ > 0)
        {
            splineChangeFrameCount_--;
            Debug.Log($"[UpdateMovement] Protecting spline change, frames remaining: {splineChangeFrameCount_}");
            if (splineChangeFrameCount_ == 0)
            {
                hasSplineChangedThisFrame_ = false;
            }
            return;
        }

        InputMovement();

        // 前フレームの位置を移動処理前に更新
        if (!isOffSpline_)
        {
            previousSplinePosition_ = splineController_.GetSplineMeshPos();
        }
        else
        {
            previousOffSplinePosition_ = transform.position;
        }

        HandleSplineMovement();

        // 地面判定をAnimationControllerに反映
        if(animController_.CurrentJumpOffsetY <= 0f && !inputs_.jump)
        {
            animController_.Grounded = Physics.CheckBox(transform.position + center_, halfExtends_, transform.rotation, groundLayer_);
        }
        else
        {
            animController_.Grounded = false;
            Debug.Log("animController_.Grounded = false;");
        }

        isFirstFrame_ = false;

        jumpControllerVariableHeight_.Tick(splineController_.EvaluationInfo.position);
        CheckSplineContainerChange();
        UpdateCamera();

        Debug.DrawRay(transform.position, offSplineVelocity_ * 1000f);
        // 空中にいる場合は下方向のSplineをチェック
        if (!animController_.Grounded)
        {
            splineController_.CheckUnderSpline();
        }
    }

    public void ApplyStompBounce(float force)
    {
        // 現在の足場位置（Spline位置）を取得
        float currentPlatformY = splineController_.EvaluationInfo.position.y;
        
        // 踏みつけジャンプを開始（forceを高さとして使用）
        jumpControllerVariableHeight_.StartStompJump(currentPlatformY, force);
        
        // アニメーション状態も更新
        if (animController_ != null)
        {
            animController_.Grounded = false;
        }
        
        Debug.Log($"Stomp bounce applied: force={force}, platformY={currentPlatformY}");
    }

    private void UpdateCamera()
    {
        // スマッシュ中はカメラ更新を停止
        if (isBeingSmashed_)
        {
            return;
        }

        if (cameraController_ != null)
        {
            cameraController_.isMovingLeft_ = splineController_.isMovingLeft;
            cameraController_.SetEvaluationInfo(splineController_.EvaluationInfo);
        }
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

    /// <summary>
    /// LaneChangeZone用の強制Spline変更処理
    /// </summary>
    /// <param name="targetSpline">変更先のSplineController</param>
    public void ForceSplineChange(SplineController targetSpline)
    {
        if (targetSpline == null) return;

        Debug.Log($"[ForceSplineChange] Starting force change to: {targetSpline.currentSplineContainer_?.name}");

        // フラグを設定して、数フレーム間の他のSpline変更処理を無効化
        hasSplineChangedThisFrame_ = true;
        splineChangeFrameCount_ = 3; // 3フレーム保護

        // 即座にSplineを変更
        splineController_.currentSplineContainer_ = targetSpline.currentSplineContainer_;
        splineController_.Progress = targetSpline.Progress;
        
        // EvaluationInfoを強制更新
        splineController_.SetSplineMeshRadius();
        
        // 位置を即座に更新
        Vector3 newPosition = splineController_.GetSplineMeshPos();
        //Debug.Log($"[ForceSplineChange] Completed. Prev position: {transform.position}, Container: {splineController_.currentSplineContainer_?.name}, Progress: {splineController_.Progress}");

        transform.position = newPosition;
        
        // 前フレームの位置も更新
        previousSplinePosition_ = newPosition;
        previousSplineContainer_ = targetSpline.currentSplineContainer_;

        // 入力と物理状態をリセット
        inputs_.jump = false;
        inputs_.move = Vector2.zero;
        animController_.ResetVerticalVelocity();
        animController_.Grounded = true;

        // オフSpline状態をリセット
        isOffSpline_ = false;
        
        // 移動関連の変数もリセット
        desiredMovement = Vector3.zero;
        actualMovement = Vector3.zero;
        splineDelta = Vector3.zero;
        splineVerticalDelta = Vector3.zero;
        knockbackForce = 0;
        
        Debug.Log($"[ForceSplineChange] Completed. New position: {transform.position}, Container: {splineController_.currentSplineContainer_?.name}, Progress: {splineController_.Progress}");
    }

    /// <summary>
    /// SplineContainer変更をチェックし、カメラに通知
    /// </summary>
    private void CheckSplineContainerChange()
    {
        // スマッシュ中はSplineContainer変更処理も停止
        if (isBeingSmashed_)
        {
            return;
        }

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
    public  void OnDamage(int damageValue)
    {
        if (!canTakeDamage_)
        {
            return; // ダメージを受けられない場合は何もしない
        }
        animController_.TakeDamage();

        hp_ -= damageValue;
        StartCoroutine(WaitCanTakeDamage());
        if (hp_ <= 0)
        {
            //OnTriggerDyingAnim();
        }
    }
    public void SideBounce(float enemyProgress)
    {
        knockbackForce = Mathf.Sqrt(knockbackLength_ * -2f * attenuationDelta_);
        knockbackDir_ = -(int)Mathf.Sign(enemyProgress - splineController_.Progress);
    }
    public void SideBounce(Vector3 enemyPos)
    {
        float dot = Vector3.Dot(splineController_.EvaluationInfo.tangent.normalized, (enemyPos - transform.position).normalized);
        knockbackDir_ = -(int)Mathf.Sign(dot);
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
        if(!isSmashed)
        {
            StartCoroutine(DyingAnim());
        }
        Debug.Log("The player is dead. Probably.");
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

    public void OnSmash(GameObject respawnPoint)
    {
        respawnPoint_ = respawnPoint;
        isBeingSmashed_ = true; // スマッシュ状態を開始
        
        // カメラにスマッシュ状態を通知
        if (cameraController_ != null)
        {
            cameraController_.SetPlayerSmashState(true);
        }
        
        StopAndReset();
        animController_.OnSmash();
        smashPlayer_.Smash(this.gameObject);
    }

    public void Respawn()
    {
        // スマッシュ状態を終了
        isBeingSmashed_ = false;
        
        // カメラにスマッシュ状態終了を通知
        if (cameraController_ != null)
        {
            cameraController_.SetPlayerSmashState(false);
        }
        
        // 入力状態を初期化
        inputs_.jump = false;
        
        // 物理状態を初期化
        animController_.ResetVerticalVelocity();
        animController_.FinishSmash();
        
        // ノックバック状態をリセット
        knockbackForce = 0;
        
        // オフスライン状態をリセット
        isOffSpline_ = false;
        offSplineVelocity_ = Vector3.zero;
        
        // 各種フラグをリセット
        canTakeDamage_ = true;
        isSmashed = false;
        
        Enable();
        
        //操作などを再度有効化する
        //splineMovementBaseなどを取得してtやevaluationInfoから...
        var respawnPointSpline = respawnPoint_.GetComponent<SplineController>();
        if (respawnPointSpline == null) return;

        splineController_.Progress = respawnPointSpline.Progress;
        
        // 位置とフレーム状態をリセット
        isFirstFrame_ = true;
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
        transform.position = previousSplinePosition_;
        previousOffSplinePosition_ = transform.position;
    }

    /// <summary>
    /// 操作や衝突判定等全て無効化、リセットする
    /// </summary>
    private void StopAndReset()
    {
        inputs_.jump = false;
        animController_.ResetVerticalVelocity();
        Disable();
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
        //Vector3 worldCenter = transform.TransformPoint(center_);
        Gizmos.DrawCube(transform.position + center_, halfExtends_);
    }
}
