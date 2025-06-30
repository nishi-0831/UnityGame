using NUnit.Framework;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
{
    [Header("����]�����čU�����Ă���G")]

    [Space(32)]
    [Header("���ɂ��U���Ԋu")]
    [SerializeField] private float attackInterval_ = 5.0f;

    [Header("���̈ړ����x")]

    [Tooltip("�p�x�ƍ��W�̕ω��ʂ͔�Ⴓ���Ă��܂���")]
    [SerializeField] private float ballMoveSpeed_ = 360.0f;

    [Header("���̉�]���x")]

    [Tooltip("�p�x�ƍ��W�̕ω��ʂ͔�Ⴓ���Ă��܂���")]
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
        //���g���灛����ɒu������ : ������t�ł͂Ȃ�����
        //distance��n����t�̒l�ɕϊ��A���g��t�Ƒ���(�����ɂ��)

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
