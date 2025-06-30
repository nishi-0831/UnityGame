using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
{
    [Header("‹…‚ğ“]‚ª‚µ‚ÄUŒ‚‚µ‚Ä‚­‚é“G")]

    [Space(32)]
    [Header("‹…‚É‚æ‚éUŒ‚ŠÔŠu")]
    [SerializeField] private float attackInterval_;

    [Header("‹…‚ÌˆÚ“®‘¬“x")]

    [Tooltip("Šp“x‚ÆÀ•W‚Ì•Ï‰»—Ê‚Í”ä—á‚³‚¹‚Ä‚¢‚Ü‚¹‚ñ")]
    [SerializeField] private float ballMoveSpeed_;

    [Header("‹…‚Ì‰ñ“]‘¬“x")]

    [Tooltip("Šp“x‚ÆÀ•W‚Ì•Ï‰»—Ê‚Í”ä—á‚³‚¹‚Ä‚¢‚Ü‚¹‚ñ")]
    [SerializeField] private float ballRollSpeed_;

    [SerializeField] private float ballOffset_;
    private EaseInterpolator easeInterpolator_;
    private GameObject ballPrefab_;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        easeInterpolator_ = GetComponent<EaseInterpolator>();
        Debug.Assert( easeInterpolator_ != null);

        easeInterpolator_.onFinished_ += GenerateBall;
    }
    protected override void Initialize()
    {
        base.Initialize();
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
            t : ballT,
            moveSpeed: ballMoveSpeed_,
            rollSpeed: ballRollSpeed_,
            isLeft:IsMovingLeft
            );
    }
}
