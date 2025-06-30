using NUnit.Framework;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
{
    [Header("球を転がして攻撃してくる敵")]


    [Header("球の設定")]
    [SerializeField] private float attackInterval_ = 5.0f;
    [SerializeField] private float ballMoveSpeed_ = 0.1f;
    [SerializeField] private float ballRollSpeed_ = 360f;
    [SerializeField] private float ballOffset_ = 30.0f;


    [SerializeField] private GameObject ballPrefab_;
    [SerializeField] private float ballRadius_ = 0.5f;
    [SerializeField]private EaseInterpolator easeInterpolator_;

    
    protected override void Initialize()
    {
        if(ballPrefab_ != null)
        {
            //ProBuilderのSphereプリミティブの半径はデフォルトで直径1なので、2で割って半径を取得
            ballRadius_ = ballPrefab_.transform.localScale.x / 2f;
        }

        easeInterpolator_ = this.GetComponent<EaseInterpolator>();
        Debug.Assert(easeInterpolator_ != null);

        easeInterpolator_.onFinished_ += GenerateBall;
        easeInterpolator_.Reset();
        easeInterpolator_.duration = attackInterval_;
        //easeInterpolator_
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
        Debug.Log($"{this.gameObject.name}:attack");
        GameObject ball = Instantiate(ballPrefab_);
        //ball.transform.position = this.transform.position;
        //自身から○○先に置きたい : ○○はtではなく距離
        //distanceを渡してtの値に変換、自身のtと増減(向きによる)

        float offsetT = splineController_.GetSplineMovementT(Mathf.Abs(ballOffset_));
        if(IsMovingLeft)
        {
            offsetT = -offsetT;
        }
        float ballT = splineController_.t_ + offsetT;

        var ballMovement = ball.GetComponent<RollingBallSplineMovement>();
        Debug.Assert( ballMovement != null );

        ballMovement.SetParam(
            splineContainer: splineController_.currentSplineContainer_,
            t: ballT,
            moveSpeed: ballMoveSpeed_,
            rollSpeed: ballRollSpeed_,
            isLeft: IsMovingLeft
            );
    }
}
