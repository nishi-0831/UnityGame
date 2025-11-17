using JetBrains.Annotations;
using MySpline;
using StarterAssets;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineController))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : SplineMovementBase
{
    [SerializeField] private float runSpeed_ = 20;
    [SerializeField] private int hp_ = 3;
    [SerializeField] float takeDamageInterval_ = 1.0f; // ダメージを受ける間隔
    [SerializeField] private bool canTakeDamage_ = true; // ダメージを受けられるかどうか

    [SerializeField] private float knockbackLength_ = 5f; // ノックバックする距離
    [SerializeField] private float knockbackForce = 0;
    //減衰率
    [SerializeField] private float attenuationDelta_ = 1f;

    [SerializeField] AnimationController animController_;
    [SerializeField] CameraController cameraController_;


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

    // Spline範囲外での移動制御
    [SerializeField] private bool isOffSpline_ = false; // Spline範囲外にいるかどうか
    [SerializeField] private Vector3 offSplineVelocity_; // Spline範囲外での移動速度
    [SerializeField] private Vector3 lastValidTangent_; // 最後の有効なタンジェント
    private bool isSmashed = false;
    [SerializeField] private SmashPlayer smashPlayer_;
    [SerializeField] private Rigidbody rb_;
    // スマッシュ状態管理
    [SerializeField] private bool isBeingSmashed_ = false;
    [SerializeField] private GameObject respawnPoint_;
    [SerializeField] private float speedInterpolateProgress_ = 0.0f;
    [SerializeField] private float speedChangeDuration_ = 1.0f;
    public JumpControllerVariableHeight jumpControllerVariableHeight_;
    private bool isDead = false;
    public float T { get { return splineController_.Progress; } }
    public int Hp { get { return hp_; } }


    
    private Action OnDamageCallback_;
    protected override void Initialize()
    {
        GameOutcomeManager.Instance?.RegisterGameClearCallback(() => animController_.GameClear());
        // タイムアップ時にゲームオーバー
        ScoreManager.Instance?.RegisterOnTimeUpCallback(OnPlayerDie);

        isDead = false;
        if (animController_ == null)
        {
            animController_ = GetComponent<AnimationController>();
        }
        //characterController_ = GetComponent<CharacterController>();
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
        Vector3 desiredMovement = Vector3.zero;
        Vector3 actualMovement = Vector3.zero;
        
        // Spline範囲内の場合
        if (currentT >= 0f || currentT <= 1f )
        {
            // 通常のSpline移動
            transform.rotation = splineController_.EvaluationInfo.rotation;
    
            if (!animController_.IsStunned && !isBeingSmashed_)
            {
                if (inputs_.move.x != 0)
                {
                    float speed = 0.0f;
                    if(animController_.IsRunning)
                    {
                        speedInterpolateProgress_ += speedChangeDuration_ * Time.deltaTime;
                        float t = EaseInterpolator.InSine(speedInterpolateProgress_);
                        speed = Mathf.Lerp(speed_, runSpeed_, t);
                    }
                    else
                    {
                        speedInterpolateProgress_ -= speedChangeDuration_ * Time.deltaTime;
                        float t = EaseInterpolator.OutSine(speedInterpolateProgress_);
                        speed = Mathf.Lerp(speed_, runSpeed_, t);
                    }

                    speedInterpolateProgress_ = Mathf.Clamp(speedInterpolateProgress_,0.0f,1.0f);
                    splineController_.Move(speed);
                }
                else
                {
                    speedInterpolateProgress_ = 0.0f;
                }
            }
            jumpControllerVariableHeight_.Tick(splineController_.EvaluationInfo.position);
        }
    }

    
    
    public void RegisterOnDamageCallback(Action onDamageAction)
    {
        OnDamageCallback_ += onDamageAction;
    }
    protected override void OnReachMaxT()
    {
        SplineContainerLink link = splineController_.currentSplineContainer_.GetComponent<SplineContainerLink>();
        HighGroundSpline highGround = splineController_.currentSplineContainer_.GetComponent<HighGroundSpline>();
        if (splineController_.currentSplineContainer_.Spline.Closed)
        {
            splineController_.Loop();
        }
        else if (link != null)
        {
            ChangeLinkedOtherSpline();

        }
        else if (highGround != null)
        {
            OnFoundNewSpline(highGround.groundSpline);
        }
    }

    protected override void OnReachMinT()
    {
        SplineContainerLink link = splineController_.currentSplineContainer_.GetComponent<SplineContainerLink>();
        HighGroundSpline highGround = splineController_.currentSplineContainer_.GetComponent<HighGroundSpline>();
        if (splineController_.currentSplineContainer_.Spline.Closed)
        {
            splineController_.Loop();
        }
        else if(link != null)
        {
            ChangeLinkedOtherSpline();

        }
        else if(highGround != null)
        {
            OnFoundNewSpline(highGround.groundSpline);
        }

    }

    public void OnFoundNewSpline(SplineContainer newSplineContainer)
    {
        // LaneChangeZoneによる強制変更の場合はスキップ
        if (hasSplineChangedThisFrame_)
        {
            Debug.Log($"[OnFoundNewSpline] Skipping due to forced change this frame. Found: {newSplineContainer?.name}");
            return;
        }

#if true
        if ( newSplineContainer == null)
        {
            return;
        }
        if (newSplineContainer.GetComponent<HighGroundSpline>() == null)
        {
            if(splineController_.currentSplineContainer_.GetComponent<HighGroundSpline>().groundSpline != newSplineContainer)
            {
                return;
            }
        }


        NativeSpline nativeSpline = new NativeSpline(newSplineContainer.Spline, newSplineContainer.transform.localToWorldMatrix);
        float3 outPos;
        float outT;
        SplineUtility.GetNearestPoint<NativeSpline>(nativeSpline, transform.position, out outPos, out outT);
        if (transform.position.y < outPos.y)
            return;

        Debug.Log($"[OnFoundNewSpline] Processing new spline: {newSplineContainer.name}");
        isOffSpline_ = false;

        float prevSplineY = splineController_.EvaluationInfo.position.y;
        // 新しいSplineに移行
        splineController_.ChangeOtherSpline(newSplineContainer);

        // 位置を新しいSplineに合わせて調整
        Vector3 newSplinePosition = splineController_.GetSplineMeshPos();
        transform.position = new Vector3(newSplinePosition.x, transform.position.y, newSplinePosition.z);

        previousSplinePosition_ = newSplinePosition;
        Debug.Log(newSplinePosition);
        jumpControllerVariableHeight_.AdjustForPlatformChange(prevSplineY, newSplinePosition.y);

        Debug.Log($"[OnFoundNewSpline] Completed spline change to: {newSplineContainer.name}");

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

        HandleSplineMovement();

        // 地面判定をAnimationControllerに反映
        if(animController_.CurrentJumpOffsetY <= 0f && !inputs_.jump)
        {
            animController_.Grounded = Physics.CheckBox(transform.position + center_, halfExtends_, transform.rotation, groundLayer_);
        }
        else
        {
            animController_.Grounded = false;
        }


        jumpControllerVariableHeight_.Tick(splineController_.EvaluationInfo.position);
        CheckSplineContainerChange();
        UpdateCamera();
        if (IsDying())
        {
            OnPlayerDie();
        }

        Debug.DrawRay(transform.position, offSplineVelocity_ * 1000f);
        // 空中にいる場合は下方向のSplineをチェック
        
        splineController_.CheckUnderSpline();

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
        
        
        // 位置を即座に更新
        Vector3 newPosition = splineController_.GetSplineMeshPos();

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
        OnDamageCallback_?.Invoke();
        StartCoroutine(WaitCanTakeDamage());
    }

    

    public void OnPlayerDie()
    {
        if (isDead == false)
        {
            isDead = true;
            GameOutcomeManager.Instance.TriggerGameOver();
            animController_.Dying();
        }
    }

    /// <summary>
    /// HPが0以下か
    /// </summary>
    /// <returns>HPが0以下ならば true</returns>
    public bool IsDying()
    {
        return hp_ <= 0;
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
        
        previousSplinePosition_ = splineController_.GetSplineMeshPos();
        transform.position = previousSplinePosition_;
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


    private bool ChangeLinkedOtherSpline()
    {
        float outOfT = Mathf.Abs(T) % 1.0f;

        SplineContainerLink link = splineController_.currentSplineContainer_.GetComponent<SplineContainerLink>();
        if (link == null)
        {
            splineController_.ClampProgress();
            return false;
        }
        else
        {
            SplineContainer moveDistContainer;
            NativeSpline moveDistNativeSpline;

            Vector3 knotPos;
            if (T < 0)
            {
                if(link.prev == null)
                {
                    splineController_.ClampProgress();
                    return false;
                }
                moveDistContainer = link.prev;
                moveDistNativeSpline = new NativeSpline(moveDistContainer.Spline, moveDistContainer.transform.localToWorldMatrix);
                
                knotPos = moveDistNativeSpline.Knots.Last().Position;
            }
            else
            {
                if (link.next == null)
                {
                    splineController_.ClampProgress();
                    return false;
                }
                moveDistContainer = link.next;
                moveDistNativeSpline = new NativeSpline(moveDistContainer.Spline, moveDistContainer.transform.localToWorldMatrix);

                knotPos = moveDistNativeSpline.Knots.First().Position;
            }

            if (transform.position.y < knotPos.y)
            {
                splineController_.ClampProgress();
                return false;
            }

            jumpControllerVariableHeight_.AdjustForPlatformChange(splineController_.EvaluationInfo.position.y, knotPos.y);

            float3 outPos;
            float outT;
            SplineUtility.GetNearestPoint<NativeSpline>(moveDistNativeSpline, transform.position, out outPos, out outT);

            
            splineController_.currentSplineContainer_ = moveDistContainer;
            splineController_.Progress = outT;

            Debug.Log("Player.y," + transform.position.y + "knot.y," + knotPos.y);
            Debug.Log("T:" + splineController_.Progress);
            return true;
        }

    }
}
