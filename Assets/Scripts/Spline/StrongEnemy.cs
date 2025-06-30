using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Splines;
[RequireComponent(typeof(EaseInterpolator))]
public class StrongEnemy : SplineMovementBase
{
    [Header("����]�����čU�����Ă���G")]

    [Space(32)]
    [Header("���ɂ��U���Ԋu")]
    [SerializeField] private float attackInterval_;

    [Header("���̈ړ����x")]

    [Tooltip("�p�x�ƍ��W�̕ω��ʂ͔�Ⴓ���Ă��܂���")]
    [SerializeField] private float ballMoveSpeed_;

    [Header("���̉�]���x")]

    [Tooltip("�p�x�ƍ��W�̕ω��ʂ͔�Ⴓ���Ă��܂���")]
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
            t : ballT,
            moveSpeed: ballMoveSpeed_,
            rollSpeed: ballRollSpeed_,
            isLeft:IsMovingLeft
            );
    }
}
