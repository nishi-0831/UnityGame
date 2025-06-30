using NUnit.Framework;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
{
    [Header("‹…‚ğ“]‚ª‚µ‚ÄUŒ‚‚µ‚Ä‚­‚é“G")]

    [Space(32)]
    [Header("‹…‚É‚æ‚éUŒ‚ŠÔŠu")]
    [SerializeField] private float attackInterval_ = 5.0f;

    [Header("‹…‚ÌˆÚ“®‘¬“x")]

    [Tooltip("Šp“x‚ÆÀ•W‚Ì•Ï‰»—Ê‚Í”ä—á‚³‚¹‚Ä‚¢‚Ü‚¹‚ñ")]
    [SerializeField] private float ballMoveSpeed_ = 360.0f;

    [Header("‹…‚Ì‰ñ“]‘¬“x")]

    [Tooltip("Šp“x‚ÆÀ•W‚Ì•Ï‰»—Ê‚Í”ä—á‚³‚¹‚Ä‚¢‚Ü‚¹‚ñ")]
    [SerializeField] private float ballRollSpeed_ = 0.1f;
    [SerializeField] private float ballOffset_ = 30.0f;
    [SerializeField] private GameObject ballPrefab_;
    [SerializeField]private EaseInterpolator easeInterpolator_;

    private void Start()
    {
        
    }

    protected override void Initialize()
    {
        base.Initialize();
        easeInterpolator_ = this.GetComponent<EaseInterpolator>();
        Debug.Assert(easeInterpolator_ != null);

        easeInterpolator_.onFinished_ += GenerateBall;
        easeInterpolator_.Reset();
        //timer_ = 0.0f;
    }
    // Update is called once per frame
    protected override void Update()
    {
    }
    protected override void UpdateMovement()
    {
        base.UpdateMovement();
        easeInterpolator_.UpdateTime();
    }
    private void GenerateBall()
    {
        if(IsActive_)
        {
            return;
        }
        GameObject ball = Instantiate(ballPrefab_);
        //ball.transform.position = this.transform.position;
        //©g‚©‚ç››æ‚É’u‚«‚½‚¢ : ››‚Ít‚Å‚Í‚È‚­‹——£
        //distance‚ğ“n‚µ‚Ät‚Ì’l‚É•ÏŠ·A©g‚Ìt‚Æ‘Œ¸(Œü‚«‚É‚æ‚é)

        float offsetT = splineController_.GetSplineMovementT(Mathf.Abs(ballOffset_));
        if(IsMovingLeft)
        {
            offsetT = -offsetT;
        }
        float ballT = splineController_.t_ - offsetT;

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
