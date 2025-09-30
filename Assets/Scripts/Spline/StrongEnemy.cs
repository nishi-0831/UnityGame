using NUnit.Framework;
using StarterAssets;
using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : PlayerInteractableBase
{
    [Header("球を転がして攻撃してくる敵")]


    [Header("球の設定")]
    [SerializeField] private float attackInterval_ = 5.0f;
    [SerializeField] private float ballMoveSpeed_ = 0.1f;
    [SerializeField] private float ballRollSpeed_ = 360f;
    [SerializeField] private float ballOffsetT_ = 0.1f;

    [SerializeField] private GameObject ballPrefab_;
    [SerializeField] private float ballRadius_ = 0.5f;
    [SerializeField]private EaseInterpolator easeInterpolator_;

    [SerializeField] private float ballLifeSpan_ = 5f;
    protected override void Initialize()
    {
        base.Initialize();
        if (ballPrefab_ != null)
        {
            //ProBuilderのSphereプリミティブの半径はデフォルトで直径1なので、2で割って半径を取得
            ballRadius_ = ballPrefab_.transform.localScale.x / 2f;
        }

        easeInterpolator_ = this.GetComponent<EaseInterpolator>();
        Debug.Assert(easeInterpolator_ != null);

        easeInterpolator_.onFinished_ += GenerateBall;
        easeInterpolator_.Reset();
        easeInterpolator_.duration = attackInterval_;
    }

    protected override void UpdateMovement()
    {
        base.UpdateMovement();
        easeInterpolator_.UpdateTime();
    }
    private void GenerateBall()
    {
        easeInterpolator_.Reset();
        if(!IsActive_)
        {
            return;
        }
        GameObject ball = Instantiate(ballPrefab_);

        float ballT;
        if(IsMovingLeft)
        {
            ballT = splineController_.Progress - ballOffsetT_;
        }
        else
        {
            ballT = splineController_.Progress + ballOffsetT_;
        }

        var ballMovement = ball.GetComponent<RollingBallSplineMovement>();
        
        Debug.Assert( ballMovement != null );

        
        ballMovement.SetParam(
            splineContainer: splineController_.currentSplineContainer_,
            t: ballT,
            moveSpeed: ballMoveSpeed_,
            rollSpeed: ballRollSpeed_,
            isLeft: IsMovingLeft,
            lifeSpan : ballLifeSpan_
            );

        animator?.SetTrigger(animIDAttack);
    }

   
    public override void OnRequestDestroy()
    {
        Destroy(gameObject);
    }
    // IPlayerInteractable実装
    public override void OnStompedCore(GameObject player)
    {
        OnDamage();
        // プレイヤーに跳ね返り効果を与える
        PlayerInteractionUtils.ApplyStompBounce(player, StompBounceForce);
    }

    public override void OnSideHitCore(GameObject player)
    {
        PlayerInteractionUtils.ApplyDamage(player,DamageToPlayer);
        PlayerInteractionUtils.ApplySideBounce(player, splineController_.Progress);
    }
}
